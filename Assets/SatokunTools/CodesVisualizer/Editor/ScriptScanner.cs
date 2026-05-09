using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ClassData
{
    public string Name;
    public string FullName;
    public string FolderName;
    public Type Type;
    public Type BaseType;
    public List<Type> Interfaces = new();
    public Dictionary<Type, List<string>> DependencyDetails = new();
    public List<Type> Dependencies => DependencyDetails.Keys.ToList();
}

public static class ScriptScanner
{
    public static List<ClassData> GetProjectClasses()
    {
        AssetDatabase.Refresh();
        var classList = new List<ClassData>();
        Assembly assembly = Assembly.Load("Assembly-CSharp");

        var allTypes = assembly.GetTypes()
            .Where(t => (t.IsClass || t.IsInterface) && !t.Name.Contains("<"))
            .ToList();

        foreach (var type in allTypes)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:Script");
            if (guids.Length == 0) continue;

            string path = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == type.Name);

            if (string.IsNullOrEmpty(path)) continue;
            if (path.Contains("SatokunTools") || path.Contains("TextMesh Pro")) continue;
            if (!path.ToLower().Contains("/script/")) continue;

            var data = new ClassData
            {
                Name = type.Name,
                FullName = type.FullName,
                FolderName = GetTopFolderName(path),
                Type = type,
                BaseType = (type.BaseType != null && allTypes.Contains(type.BaseType)) ? type.BaseType : null,
                Interfaces = type.GetInterfaces().Where(i => allTypes.Contains(i)).ToList()
            };

            try
            {
                string scriptContent = File.ReadAllText(path);
                string cleanContent = Regex.Replace(scriptContent, @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)", "");

                foreach (var otherType in allTypes)
                {
                    if (otherType == type) continue;

                    if (Regex.IsMatch(cleanContent, $@"\b{otherType.Name}\b"))
                    {
                        if (!data.DependencyDetails.ContainsKey(otherType))
                            data.DependencyDetails[otherType] = new List<string>();

                        // --- ★ ここから「特大・詳細解析」の核心部 ---

                        // まず最初に「どのクラスか」をヘッダーとして入れる
                        data.DependencyDetails[otherType].Add($"[ Class: {otherType.Name} ]");

                        // 相手のメンバ（変数、プロパティ、メソッド）を全部チェック
                        var members = otherType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

                        foreach (var member in members)
                        {
                            // アクセサ（get/set）やコンストラクタはノイズになるので飛ばす
                            if (member.Name.StartsWith("get_") || member.Name.StartsWith("set_") || member.Name == ".ctor") continue;

                            // 自分のコード内に、相手のメンバ名（変数名やメソッド名）が含まれているか
                            if (Regex.IsMatch(cleanContent, $@"\b{member.Name}\b"))
                            {
                                string detail = "";

                                // --- 種類別に詳細文字列を組み立てる ---
                                if (member is FieldInfo f)
                                {
                                    string prefix = f.IsStatic ? "Static Variable" : "Variable";
                                    detail = $"{prefix} {f.Name} : {f.FieldType.Name}";
                                }
                                else if (member is PropertyInfo p)
                                {
                                    detail = $"Variable {p.Name} : {p.PropertyType.Name}";
                                }
                                else if (member is MethodInfo m)
                                {
                                    string prefix = m.IsStatic ? "Static Method" : "Method";
                                    // 引数の型と名前を取得
                                    string args = string.Join(", ", m.GetParameters().Select(pa => $"{pa.ParameterType.Name} {pa.Name}"));
                                    detail = $"{prefix} {m.Name}({args}) : {m.ReturnType.Name}";
                                }

                                // 重複していなければ追加
                                if (!string.IsNullOrEmpty(detail) && !data.DependencyDetails[otherType].Contains(detail))
                                {
                                    data.DependencyDetails[otherType].Add(detail);
                                }
                            }
                        }

                        // クラス名は出てるけどメンバ名が拾えなかった時用
                        if (data.DependencyDetails[otherType].Count <= 1) // ヘッダーのみの状態
                        {
                            data.DependencyDetails[otherType].Add("(Class Type Reference Only)");
                        }
                    }
                }
            }
            catch (Exception e) { Debug.LogWarning($"Failed to read {path}: {e.Message}"); }

            classList.Add(data);
        }
        return classList;
    }

    private static string GetTopFolderName(string path)
    {
        string relative = path.Replace("Assets/Script/", "");
        string dir = Path.GetDirectoryName(relative).Replace("\\", "/");
        return string.IsNullOrEmpty(dir) ? "Script" : "Script/" + dir;
    }
}