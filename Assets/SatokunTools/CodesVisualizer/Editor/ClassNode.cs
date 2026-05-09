using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ClassNode : Node
{
    // ★追加：これがないと ClassGraphView から詳細データ（Typeなど）が見えません
    public ClassData Data { get; private set; }
    public string FullName { get; private set; }
    public bool IsStatic { get; private set; }

    public ClassNode(ClassData data, Vector2 pos, Action<ClassNode> onSelect)
    {
        // ★修正：渡されたデータをプロパティに保存する
        this.Data = data;

        this.FullName = data.FullName;
        // C#において「abstract かつ sealed」なクラスは static クラスとして扱われます
        this.IsStatic = data.Type.IsAbstract && data.Type.IsSealed;
        this.title = data.Name;

        SetupStyles(data);
        SetupPorts();
        SetupContents(data);

        this.RegisterCallback<MouseDownEvent>(e => {
            if (e.clickCount == 2)
            {
                OpenScript(data.Name);
            }
            else
            {
                onSelect?.Invoke(this);
            }
        });

        this.expanded = true;
        this.RefreshExpandedState();
        this.RefreshPorts();
        this.SetPosition(new Rect(pos, new Vector2(280, 160)));
    }

    // --- 以下、SetupStyles 等のメソッドは監督の書いた通りでOK！ ---
    private void SetupStyles(ClassData data)
    {
        var titleLabel = this.titleContainer.Q<Label>();
        if (titleLabel != null)
        {
            titleLabel.style.color = Color.black;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 18;
        }

        // ★修正：色の優先順位
        if (this.IsStatic)
        {
            // Staticクラスなら「警告色」の黄色
            this.titleContainer.style.backgroundColor = new Color(1f, 0.8f, 0f);
        }
        else if (data.Type.IsInterface)
        {
            // インターフェースなら水色
            this.titleContainer.style.backgroundColor = new Color(0.6f, 0.8f, 1f);
        }
        else
        {
            // 通常クラスなら薄オレンジ
            this.titleContainer.style.backgroundColor = new Color(1f, 0.85f, 0.4f);
        }

        this.style.borderLeftWidth = this.style.borderRightWidth = this.style.borderTopWidth = this.style.borderBottomWidth = 2f;
        this.style.borderLeftColor = this.style.borderRightColor = this.style.borderTopColor = this.style.borderBottomColor = Color.black;
        this.style.backgroundColor = Color.white;
    }

    private void SetupPorts()
    {
        var inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        var outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
        outputPort.portName = "Out";
        outputContainer.Add(outputPort);

        this.Query<Label>().ForEach(l => {
            l.style.color = Color.black;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.style.fontSize = 14;
        });
    }

    private void SetupContents(ClassData data)
    {
        // --- Variables ---
        var varFold = new Foldout { text = "Variables", value = false };
        FormatFoldout(varFold);

        // ★修正：BindingFlags.Static を追加して、GlobalSettings内の静的変数も表示する
        var fieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        foreach (var f in data.Type.GetFields(fieldFlags))
        {
            string prefix = f.IsStatic ? "Static" : (f.IsPublic ? "●" : "○");
            varFold.Add(CreateDataLabel($"{prefix} {f.Name} : {f.FieldType.Name}"));
        }
        extensionContainer.Add(varFold);

        // --- Methods ---
        var methodFold = new Foldout { text = "Methods", value = false };
        FormatFoldout(methodFold);

        // ★修正：BindingFlags.Static を追加
        var methodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var methods = data.Type.GetMethods(methodFlags);
        foreach (var m in methods)
        {
            if (m.IsSpecialName) continue;
            string prefix = m.IsStatic ? "Static" : (m.IsPublic ? "●" : "○");
            string args = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            methodFold.Add(CreateDataLabel($"{prefix} {m.Name}({args}) : {m.ReturnType.Name}"));
        }
        extensionContainer.Add(methodFold);
    }

    private void FormatFoldout(Foldout f)
    {
        var label = f.Q<Label>();
        if (label != null)
        {
            label.style.color = Color.black;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = 15;
        }
    }

    private Label CreateDataLabel(string text) => new Label(text)
    {
        style = { color = Color.black, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 15 }
    };

    private void OpenScript(string name)
    {
        var guids = AssetDatabase.FindAssets($"{name} t:MonoScript");
        if (guids.Length > 0)
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guids[0])));
    }
}