using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Unityの標準Edgeと衝突しないように別名を定義
using GraphEdge = UnityEditor.Experimental.GraphView.Edge;

public class ClassGraphView : GraphView
{
    private GraphAnalyzer _analyzer = new();
    private LayoutHandler _layout = new();
    private Dictionary<string, ClassNode> _nodeDictionary = new();
    private string _currentFilter = "All";

    // ★ 司令部直属の「浮遊ラベル」：ノードより上の階層で管理する
    private Label _floatingLabel;

    public ClassGraphView()
    {
        style.backgroundColor = new StyleColor(new Color(0.95f, 0.93f, 0.88f, 1f));
        var grid = new GridBackground();
        Insert(0, grid);

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        style.flexGrow = 1;

        CreateToolbar();
        SetupFloatingLabel(); // ★ ラベルの初期設定

        // 背景クリックでハイライト解除
        this.RegisterCallback<PointerDownEvent>(evt => {
            var target = evt.target as VisualElement;
            if (target == null) return;
            bool isFolderPart = target.GetFirstAncestorOfType<Group>() != null;
            bool isNodePart = target.GetFirstAncestorOfType<Node>() != null;
            if ((target is ClassGraphView || target is GridBackground || isFolderPart) && !isNodePart)
                ResetHighlight();
        });

        // 配置保存
        graphViewChanged = (changes) => {
            if (changes.movedElements != null)
            {
                foreach (var element in changes.movedElements)
                {
                    if (element is ClassNode node)
                        _layout.SaveNode(node.FullName, node.GetPosition().position);
                    else if (element is Group group)
                        _layout.SaveGroup(group.title, group.GetPosition().position);
                }
            }
            return changes;
        };

        GenerateGraph();
    }

    // ★ ラベルを「線の外」の最前面レイヤーに配置する
    private void SetupFloatingLabel()
    {
        _floatingLabel = new Label
        {
            pickingMode = PickingMode.Ignore, // ★ エラー修正：styleの外で指定
            style = {
                position = Position.Absolute,
                backgroundColor = new Color(0, 0, 0, 0.85f),
                color = new Color(0.2f, 1f, 0.2f),
                fontSize = 16,
                unityFontStyleAndWeight = FontStyle.Bold,
                paddingLeft = 10, paddingTop = 8, paddingRight = 10, paddingBottom = 8,
                borderBottomLeftRadius = 5, borderBottomRightRadius = 5,
                borderTopLeftRadius = 5, borderTopRightRadius = 5,
                visibility = Visibility.Hidden,
                whiteSpace = WhiteSpace.Normal,
                maxWidth = 450
            }
        };
        // contentViewContainer に入れることで、ノードより常に上の層に表示される
        contentViewContainer.Add(_floatingLabel);
    }

    // ★ エッジ（線）から呼ばれる表示・非表示命令
    public void ShowFloatingLabel(List<string> details, Vector2 pos)
    {
        if (details == null || details.Count == 0) return;
        _floatingLabel.text = string.Join("\n", details);
        _floatingLabel.style.visibility = Visibility.Visible;
        _floatingLabel.transform.position = pos - new Vector2(100, 30); // 中央寄せ
        _floatingLabel.BringToFront(); // 物理的に最前面へ
    }

    public void HideFloatingLabel() => _floatingLabel.style.visibility = Visibility.Hidden;

    private void CreateToolbar()
    {
        var toolbar = new Toolbar();
        toolbar.Add(new Button(() => GenerateGraph()) { text = "🔄 Analyze" });
        toolbar.Add(new Button(() => _layout.SaveAsset()) { text = "💾 Save Layout" });
        toolbar.Add(new Button(() => ExportToPNG()) { text = "📷 Export PNG" });

        var filterMenu = new ToolbarMenu { text = $"View: {_currentFilter}" };
        filterMenu.menu.AppendAction("All", a => { _currentFilter = "All"; filterMenu.text = "View: All"; GenerateGraph(); });

        _analyzer.Refresh();
        var folders = _analyzer.AllClasses.Select(c => c.FolderName).Distinct().OrderBy(f => f);
        foreach (var f in folders)
            filterMenu.menu.AppendAction(f, a => { _currentFilter = a.name; filterMenu.text = $"View: {a.name}"; GenerateGraph(); });

        toolbar.Add(filterMenu);
        Add(toolbar);
    }

    public void GenerateGraph()
    {
        graphElements.ForEach(RemoveElement);
        _nodeDictionary.Clear();
        _analyzer.Refresh();
        SetupFloatingLabel(); // 再生成時にもラベルを初期化

        var filteredClasses = _analyzer.AllClasses
            .Where(c => _currentFilter == "All" || c.FolderName == _currentFilter)
            .ToList();

        var folderGroups = filteredClasses.GroupBy(c => c.FolderName);

        float curX = 0, curY = 0, nextY = 0;
        int folderIndex = 0;

        foreach (var folder in folderGroups)
        {
            var uiGroup = new Group { title = folder.Key };
            uiGroup.style.backgroundColor = folder.Key.Contains("Global")
                ? new StyleColor(new Color(1f, 0.8f, 0f, 0.1f))
                : new StyleColor(new Color(0, 0, 0, 0.05f));

            AddElement(uiGroup);

            if (_layout.TryGetGroupPos(folder.Key, out Vector2 groupPos))
                uiGroup.SetPosition(new Rect(groupPos, Vector2.zero));

            int i = 0;
            foreach (var data in folder)
            {
                if (!_layout.TryGetNodePos(data.FullName, out Vector2 pos))
                {
                    Vector2 basePos = _layout.TryGetGroupPos(folder.Key, out Vector2 gPos) ? gPos : new Vector2(curX, curY);
                    pos = new Vector2(basePos.x + (i % 5) * 380f + 60f, basePos.y + (i / 5) * 220f + 100f);
                }

                var node = new ClassNode(data, pos, n => HighlightConnections(n));
                _nodeDictionary[data.FullName] = node;

                AddElement(node);
                uiGroup.AddElement(node);
                i++;
            }

            int rows = Mathf.CeilToInt(i / 5f);
            nextY = Mathf.Max(nextY, curY + rows * 220f + 300f);
            if (++folderIndex % 2 == 0) { curX = 0; curY = nextY; } else { curX += 2000f; }
        }

        foreach (var data in filteredClasses)
        {
            if (!_nodeDictionary.TryGetValue(data.FullName, out var fromN)) continue;
            if (data.BaseType != null && _nodeDictionary.TryGetValue(data.BaseType.FullName, out var bN))
                Link(fromN, bN, new List<string> { "[ Inheritance ]" }, Color.black);

            // --- ★ 修正：依存関係（詳細付き）の色の優先順位を強化する ---
            foreach (var depPair in data.DependencyDetails)
            {
                if (_nodeDictionary.TryGetValue(depPair.Key.FullName, out var dN))
                {
                    // 判定1：送り側（data）または受け取り側（depPair.Key）が static か？
                    bool fromStatic = data.Type.IsAbstract && data.Type.IsSealed; // 送信側が世界の掟（Global）
                    bool toStatic = depPair.Key.IsAbstract && depPair.Key.IsSealed; // 受信側が世界の掟

                    bool isCycle = _analyzer.Cycles.Contains((data.FullName, depPair.Key.FullName));

                    // ★ 修正：色の決定ロジック。
                    // Staticクラスが関わる通信なら、継承やType Referenceを無視して「Orange (Static Dependency)」を最優先する。
                    Color edgeColor;
                    if (fromStatic || toStatic)
                    {
                        // Static Dependency はオレンジ。透過度は普段の stealth 仕様 (0.1f) に合わせる。
                        edgeColor = new Color(1f, 0.6f, 0f, 0.1f);
                    }
                    else if (isCycle)
                    {
                        // 循環参照は警告（赤）。0.1fだと見えなくなるので、薄く赤くする。
                        edgeColor = new Color(1f, 0f, 0f, 0.2f);
                    }
                    else
                    {
                        // 通常の依存は薄いグレー（Reference / Type Reference）。凡例のReference dashed に合わせる。
                        edgeColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                    }

                    // この色を PersistentColor として Link に渡す
                    Link(fromN, dN, depPair.Value, edgeColor);
                }
            }
        }
        CreateLegend();
    }

    private void Link(ClassNode outN, ClassNode inN, List<string> details, Color col)
    {
        var edge = new BlueJEdge
        {
            output = outN.outputContainer[0] as Port,
            input = inN.inputContainer[0] as Port,
            UsageDetails = details
        };
        edge.input.Connect(edge); edge.output.Connect(edge);
        edge.PersistentColor = col;
        edge.style.opacity = 0.1f;
        AddElement(edge);
    }

    private void HighlightConnections(ClassNode selectedNode)
    {
        edges.ForEach(e => {
            var ge = (BlueJEdge)e;
            ge.style.opacity = 0.05f;
            ge.PersistentColor = new Color(0.6f, 0.6f, 0.6f);
            ge.MarkDirtyRepaint();
        });

        HideFloatingLabel();

        foreach (var edge in edges.Cast<BlueJEdge>())
        {
            bool isConnected = edge.input.node == selectedNode || edge.output.node == selectedNode;
            if (isConnected)
            {
                edge.style.opacity = 0.8f;

                var fromNode = (ClassNode)edge.output.node;
                var toNode = (ClassNode)edge.input.node;

                // ★ ここを修正：ClassData ではなく Data （または _data など）にする
                // もし ClassNode 内の変数が Data ならこれで行けます
                var fromData = fromNode.Data;
                var toData = toNode.Data;

                bool isStaticLink = (fromData.Type.IsAbstract && fromData.Type.IsSealed) ||
                                   (toData.Type.IsAbstract && toData.Type.IsSealed);

                if (isStaticLink)
                {
                    edge.PersistentColor = new Color(1f, 0.6f, 0f); // オレンジ
                }
                else
                {
                    edge.PersistentColor = (edge.output.node == selectedNode) ? Color.red : Color.blue;
                }

                edge.MarkDirtyRepaint();
            }
        }
    }
    private void ResetHighlight()
    {
        edges.ForEach(e => {
            var ge = (BlueJEdge)e;
            ge.style.opacity = 1f;
            ge.PersistentColor = new Color(0.5f, 0.5f, 0.5f);
            ge.MarkDirtyRepaint();
        });
        nodes.ForEach(n => n.style.opacity = 1f);
        HideFloatingLabel();
    }

    private void ExportToPNG() { /* 既存のコード */ }

    private void CreateLegend()
    {
        var legend = new VisualElement();
        legend.style.position = Position.Absolute;
        legend.style.right = 20; legend.style.top = 40;
        legend.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.85f));
        legend.style.paddingLeft = 10; legend.style.paddingRight = 10;
        legend.style.paddingTop = 8; legend.style.paddingBottom = 8;
        legend.style.borderBottomLeftRadius = 10;
        legend.Add(new Label("▼ Relationship") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
        legend.Add(CreateLegendItem("━ Inheritance", Color.black));
        legend.Add(CreateLegendItem("╌ Reference", new Color(0.5f, 0.5f, 0.5f)));
        legend.Add(new Label("▼ Selection") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });
        legend.Add(CreateLegendItem("▶ Provides Data (Outgoing)", Color.red));
        legend.Add(CreateLegendItem("◀ Receives Data (Incoming)", Color.blue));
        legend.Add(CreateLegendItem("Static Dependency", new Color(1f, 0.6f, 0f)));
        Add(legend);
    }

    private VisualElement CreateLegendItem(string text, Color color)
    {
        var item = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 2 } };
        var line = new VisualElement { style = { width = 12, height = 3, backgroundColor = new StyleColor(color), alignSelf = Align.Center, marginRight = 5 } };
        item.Add(line); item.Add(new Label(text) { style = { color = Color.white, fontSize = 10 } });
        return item;
    }

    public override List<Port> GetCompatiblePorts(Port start, NodeAdapter adapter) =>
        ports.ToList().Where(p => p.node != start.node && p.direction != start.direction).ToList();
}

public class BlueJEdge : UnityEditor.Experimental.GraphView.Edge
{
    public Color PersistentColor = new(0.5f, 0.5f, 0.5f);
    public List<string> UsageDetails = new();

    public override void OnSelected()
    {
        base.OnSelected();
        this.style.opacity = 1f;
        this.BringToFront();

        if (GetFirstAncestorOfType<ClassGraphView>() is ClassGraphView view)
        {
            // edgeControl が無い場合は何もしない（ガード）
            if (edgeControl == null) return;
            Vector2 mid = (edgeControl.from + edgeControl.to) * 0.5f;
            view.ShowFloatingLabel(UsageDetails, mid);
        }
    }

    public override void OnUnselected()
    {
        base.OnUnselected();
        this.style.opacity = 0.1f;
        if (GetFirstAncestorOfType<ClassGraphView>() is ClassGraphView view)
            view.HideFloatingLabel();
    }

    public override bool UpdateEdgeControl()
    {
        base.UpdateEdgeControl();

        // --- ★ 3段構えの Null ガード ---

        // 1. edgeControl 自体が生成されていない場合は即終了
        if (edgeControl == null) return false;

        // 2. 入口(input)か出口(output)が繋がっていない場合も計算不能なので終了
        if (input == null || output == null) return false;

        edgeControl.inputColor = PersistentColor;
        edgeControl.outputColor = PersistentColor;

        // 3. 制御点(controlPoints)がまだ初期化されていない場合も終了
        if (edgeControl.controlPoints == null || edgeControl.controlPoints.Length < 4)
            return false;

        // --- ★ 座標計算（ここに来る頃には安全） ---
        Vector2 start = edgeControl.from;
        Vector2 end = edgeControl.to;

        // かくかく配線ロジック
        float midX = start.x + (end.x - start.x) * 0.5f;
        edgeControl.controlPoints[1] = new Vector2(midX, start.y);
        edgeControl.controlPoints[2] = new Vector2(midX, end.y);

        return true;
    }
}