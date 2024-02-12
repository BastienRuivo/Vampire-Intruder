using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public float timeOffset;
    public Vector3 posOffset;
    private Vector3 velocity;

    void Update()
    {
        // Obtenir la position actuelle de la caméra
        Vector3 targetPos = transform.position;

        // Définir la position X et Y de la cible sur celle du joueur
        targetPos.x = player.transform.position.x + posOffset.x;
        targetPos.y = player.transform.position.y + posOffset.y;

        // Appliquer la position de la cible à la caméra tout en conservant sa position Z actuelle
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, timeOffset); 
    }
}