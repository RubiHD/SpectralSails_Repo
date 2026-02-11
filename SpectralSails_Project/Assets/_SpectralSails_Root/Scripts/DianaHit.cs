using UnityEngine;

public class DianaGolpeConShake : MonoBehaviour
{
    private Animator anim;

    private Vector3 escalaOriginal;
    private Vector3 posicionOriginal;

    public float aumentoEscala = 1.2f;
    public float duracionShake = 0.15f;
    public float intensidadShake = 0.05f;

    void Start()
    {
        anim = GetComponent<Animator>();
        escalaOriginal = transform.localScale;
        posicionOriginal = transform.localPosition;
    }

    // Este metodo lo llamas desde tu script de ataque
    public void RecibirGolpe()
    {
        anim.SetTrigger("Golpe");

        transform.localScale = escalaOriginal * aumentoEscala;

        StartCoroutine(Shake());

        Invoke(nameof(VolverEscalaOriginal), duracionShake);
    }

    private System.Collections.IEnumerator Shake()
    {
        float tiempo = 0f;

        while (tiempo < duracionShake)
        {
            float x = Random.Range(-intensidadShake, intensidadShake);
            float y = Random.Range(-intensidadShake, intensidadShake);

            transform.localPosition = posicionOriginal + new Vector3(x, y, 0f);

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = posicionOriginal;
    }

    private void VolverEscalaOriginal()
    {
        transform.localScale = escalaOriginal;
    }
}
