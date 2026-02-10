using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform jugador;

    [Header("Configuración")]
    public float suavizado = 5f;

    [Header("Tilemap para límites")]
    public Tilemap tilemap; // Arrastra aquí tu tilemap

    private float limiteIzquierdo;
    private float limiteDerecho;
    private float limiteInferior;
    private float limiteSuperior;

    private float altoCamara;
    private float anchoCamara;

    void Start()
    {
        CalcularLimitesDesdeTilemap();
    }

    void CalcularLimitesDesdeTilemap()
    {
        if (tilemap == null)
        {
            Debug.LogError("¡No hay Tilemap asignado!");
            return;
        }

        // Obtener los límites del tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Convertir a coordenadas del mundo
        Vector3 minTilemap = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 maxTilemap = tilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));

        // Calcular el tamaño visible de la cámara
        Camera cam = GetComponent<Camera>();
        altoCamara = cam.orthographicSize;
        anchoCamara = altoCamara * cam.aspect;

        // Calcular límites (para que la cámara no muestre más allá del tilemap)
        limiteIzquierdo = minTilemap.x + anchoCamara;
        limiteDerecho = maxTilemap.x - anchoCamara;
        limiteInferior = minTilemap.y + altoCamara;
        limiteSuperior = maxTilemap.y - altoCamara;

        Debug.Log($"Límites calculados - Izq: {limiteIzquierdo}, Der: {limiteDerecho}, Inf: {limiteInferior}, Sup: {limiteSuperior}");
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        // Posición objetivo (seguir al jugador, pero mantener Z en -10)
        Vector3 posicionDeseada = new Vector3(jugador.position.x, jugador.position.y, -10);

        // Aplicar suavizado
        Vector3 posicionSuave = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);

        // Limitar la posición de la cámara a los bordes del tilemap
        posicionSuave.x = Mathf.Clamp(posicionSuave.x, limiteIzquierdo, limiteDerecho);
        posicionSuave.y = Mathf.Clamp(posicionSuave.y, limiteInferior, limiteSuperior);

        transform.position = posicionSuave;
    }

    // Para visualizar los límites en el editor
    void OnDrawGizmos()
    {
        if (tilemap == null) return;

        Gizmos.color = Color.red;

        // Dibujar rectángulo de los límites
        Vector3 centro = new Vector3(
            (limiteIzquierdo + limiteDerecho) / 2,
            (limiteInferior + limiteSuperior) / 2,
            0
        );

        Vector3 tamaño = new Vector3(
            limiteDerecho - limiteIzquierdo,
            limiteSuperior - limiteInferior,
            0
        );

        Gizmos.DrawWireCube(centro, tamaño);
    }
}
