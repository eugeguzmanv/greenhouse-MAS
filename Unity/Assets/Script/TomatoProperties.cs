using UnityEngine;

[DisallowMultipleComponent]
public class TomatoProperties : MonoBehaviour
{
    [Header("Features (match server names)")]
    [Tooltip("0..1 scale (or whatever scale you used for training)")]
    public float fruit_redness = 0.0f;
    public float fruit_greenness = 0.0f;
    public float leaf_health = 1.0f;
    public float spot_count = 0f;
    public float spot_darkness = 0f;
    public float surface_texture = 0f;
    public float size = 0.5f;
    public float stem_brownness = 0f;
    public int x_coordinate = 0;
    public int y_coordinate = 0;

    [Header("Visual / runtime")]
    public MeshRenderer overrideRenderer; // opcional: asigna si el MeshRenderer no está en la raíz
    [Tooltip("Color to show when marked for cutting")]
    public Color cutColor = Color.red;
    [Tooltip("Color to show on scanned (non-cut)")]
    public Color scannedColor = Color.blue;

    // Método público para acceder a todos los atributos del tomate
    public void GetTomatoData()
    {
        string tomatoInfo = $"===== DATOS DEL TOMATE =====\n" +
                           $"Fruit Redness: {fruit_redness}\n" +
                           $"Fruit Greenness: {fruit_greenness}\n" +
                           $"Leaf Health: {leaf_health}\n" +
                           $"Spot Count: {spot_count}\n" +
                           $"Spot Darkness: {spot_darkness}\n" +
                           $"Surface Texture: {surface_texture}\n" +
                           $"Size: {size}\n" +
                           $"Stem Brownness: {stem_brownness}\n" +
                           $"X Coordinate: {x_coordinate}\n" +
                           $"Y Coordinate: {y_coordinate}\n" +
                           $"========================";
        Debug.Log(tomatoInfo);
    }

    // Called by RayCast when server decides to cut
    public void CutTomato()
    {
        // Ejemplo simple: desactivar el objeto o reproducir animación
        // Puedes reemplazar por animación, spawn de piezas, etc.
        gameObject.SetActive(false);
        Debug.Log($"Tomato cortado.");
    }

    // Helper to color the tomato (main thread only)
    public void ApplyColor(Color color)
    {
        MeshRenderer mr = overrideRenderer != null ? overrideRenderer : GetComponent<MeshRenderer>();
        if (mr == null) mr = GetComponentInChildren<MeshRenderer>();
        if (mr == null) return;

        // Esto crea instancias de materiales para no modificar SharedMaterial globalmente.
        var mats = mr.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].color = color;
        }
        mr.materials = mats;
    }
}
