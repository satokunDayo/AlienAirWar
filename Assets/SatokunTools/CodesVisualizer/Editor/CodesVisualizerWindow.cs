using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ClassVisualizerWindow : EditorWindow
{
    // Adds a new menu item under "SatoTools" in the Unity Editor menu bar.
    // Selecting this menu item will open the Class Visualizer window.
    [MenuItem("SatoTools/Class Visualizer")]
    public static void Open() => GetWindow<ClassVisualizerWindow>("Class Visualizer");

    private void CreateGUI()
    {
        // Create an instance of the custom GraphView that visualizes class relationships.
        // This is the main interactive canvas where nodes and edges are displayed.
        var graphView = new ClassGraphView();

        // Make the GraphView automatically resize to fill the entire EditorWindow.
        // Without this, the view might not expand properly when the window is resized.
        graphView.StretchToParentSize();

        // Add the GraphView to the root visual element of the EditorWindow.
        // This attaches the UI to the window so it becomes visible and interactive.
        rootVisualElement.Add(graphView);
    }
}
