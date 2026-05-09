using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------
// NodePositionData
// -----------------------------------------------------------------------------
// This ScriptableObject acts as a persistent storage container for the layout
// state of the ClassGraphView. It stores:
//   - Node positions (per class)
//   - Folder group positions
//
// WHY ScriptableObject?
// ---------------------
// Unity's ScriptableObject is ideal for editor-only persistent data because:
//
// 1. It is serialized as a standalone .asset file.
//    - Stored in YAML format (text-based)
//    - Version-controlled (Git-friendly)
//    - Human-readable
//
// 2. It survives domain reloads and editor restarts.
//    - Unlike static fields (cleared on domain reload)
//    - Unlike EditorWindow state (not serialized)
//
// 3. It participates in UnityÅfs serialization system.
//    - Fields are automatically saved when marked dirty
//    - Supports Undo/Redo if integrated
//
// INTERNAL SERIALIZATION DETAILS:
// -------------------------------
// - Unity serializes public fields and [SerializeField] private fields.
// - Supported types include:
//     * Primitive types
//     * Unity structs (Vector2, Vector3, Color, etc.)
//     * Lists of supported types
// - ScriptableObject instances are referenced by GUID in .meta files.
//
// MEMORY MODEL:
// -------------
// - ScriptableObjects live in UnityÅfs native C++ memory, not managed heap.
// - Only the managed wrapper is in C#.
// - This makes them lightweight and GC-friendly.
//
// This class intentionally uses parallel lists instead of Dictionary because:
// - Unity cannot serialize Dictionary<TKey, TValue>
// - Parallel lists are the standard workaround
// -----------------------------------------------------------------------------
public class NodePositionData : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Node Position Storage
    // -------------------------------------------------------------------------
    // These two lists form a serialized dictionary:
    //   keys[i]   Å® class full name
    //   values[i] Å® saved Vector2 position
    //
    // WHY NOT DICTIONARY?
    // -------------------
    // Unity cannot serialize Dictionary because:
    // - Dictionaries contain hash buckets
    // - Hash buckets contain unmanaged pointers
    // - UnityÅfs serializer requires deterministic, flat data
    //
    // Parallel lists guarantee:
    // - Deterministic ordering
    // - Full serialization support
    // -------------------------------------------------------------------------
    public List<string> keys = new();
    public List<Vector2> values = new();

    // Same structure for folder group positions.
    public List<string> folderKeys = new();
    public List<Vector2> folderValues = new();

    // -------------------------------------------------------------------------
    // SavePosition
    // -------------------------------------------------------------------------
    // Stores or updates the position of a class node.
    //
    // PERFORMANCE NOTES:
    // ------------------
    // - List.IndexOf() is O(n)
    // - n = number of classes in the project
    // - For typical Unity projects (< 500 classes), this is trivial
    //
    // - If the project grows large, a runtime Dictionary cache could be used
    //   to accelerate lookups, but the serialized format must remain lists.
    //
    // SERIALIZATION NOTES:
    // --------------------
    // - After modifying the lists, the ScriptableObject must be marked dirty
    //   via EditorUtility.SetDirty() to ensure Unity writes it to disk.
    // -------------------------------------------------------------------------
    public void SavePosition(string key, Vector2 pos)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
            values[index] = pos;     // Update existing entry
        else
        {
            keys.Add(key);           // Add new entry
            values.Add(pos);
        }
    }

    // -------------------------------------------------------------------------
    // TryGetPosition
    // -------------------------------------------------------------------------
    // Attempts to retrieve a saved position for a class node.
    //
    // DESIGN NOTES:
    // -------------
    // - Using TryGet pattern avoids exceptions and keeps the API clean.
    // - Returning Vector2.zero for missing entries is safe because:
    //     * GraphView will auto-place nodes when no saved position exists.
    // -------------------------------------------------------------------------
    public bool TryGetPosition(string key, out Vector2 pos)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            pos = values[index];
            return true;
        }

        pos = Vector2.zero;
        return false;
    }

    // -------------------------------------------------------------------------
    // SaveFolderPosition
    // -------------------------------------------------------------------------
    // Same logic as SavePosition, but for folder groups.
    //
    // Folder groups represent top-level directories in the project hierarchy.
    // Their positions are saved independently from nodes.
    //
    // WHY SAVE FOLDER POSITIONS?
    // --------------------------
    // - GraphView Groups are draggable containers.
    // - Users may rearrange folder groups visually.
    // - Persisting their positions preserves the mental model of the layout.
    // -------------------------------------------------------------------------
    public void SaveFolderPosition(string key, Vector2 pos)
    {
        int index = folderKeys.IndexOf(key);
        if (index >= 0)
            folderValues[index] = pos;
        else
        {
            folderKeys.Add(key);
            folderValues.Add(pos);
        }
    }

    // -------------------------------------------------------------------------
    // TryGetFolderPosition
    // -------------------------------------------------------------------------
    // Retrieves the saved position of a folder group.
    //
    // NOTE:
    // - Folder groups are optional; if not found, return Vector2.zero.
    // - GraphView will auto-place them if no saved position exists.
    // -------------------------------------------------------------------------
    public bool TryGetFolderPosition(string key, out Vector2 pos)
    {
        int index = folderKeys.IndexOf(key);
        if (index >= 0)
        {
            pos = folderValues[index];
            return true;
        }

        pos = Vector2.zero;
        return false;
    }
}
