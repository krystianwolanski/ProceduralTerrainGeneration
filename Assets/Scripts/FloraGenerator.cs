using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraGenerator : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> bushModels = new List<GameObject>();

    [SerializeField]
    [Range(1,100)]
    private int percentChance;

    public void GenerateFlora(GameObject tile)
    {

        var randomNumber = Random.Range(1, 100);

        if (randomNumber <= percentChance)
        {

            RaycastHit raycastHit;
            Physics.Raycast(
                    new Ray(tile.transform.position + Vector3.up * 10f, Vector3.down),
                    out raycastHit,
                    20
                );

            Debug.DrawRay(tile.transform.position + Vector3.up * 5f, Vector3.left * 20,Color.red, 10000);

            RaycastHit rayCastLeftHit, rayCastRightHit, rayCastForward, rayCastBack;

            var leftHit = Physics.Raycast(
                new Ray(tile.transform.position + Vector3.up * 5f, Vector3.left),
                out rayCastLeftHit,
                20
                );

            Debug.DrawRay(tile.transform.position + Vector3.up * 5f, Vector3.right * 20, Color.red, 10000);

            var rightHit = Physics.Raycast(
                new Ray(tile.transform.position + Vector3.up * 5f, Vector3.right),
                out rayCastRightHit,
                20
                );

            Debug.DrawRay(tile.transform.position + Vector3.up * 5f, Vector3.forward * 20, Color.red, 10000);

            var forwardHit = Physics.Raycast(
                new Ray(tile.transform.position + Vector3.up * 5f, Vector3.forward),
                out rayCastForward,
                20
                );

            Debug.DrawRay(tile.transform.position + Vector3.up * 5f, Vector3.back * 20, Color.red, 10000);

            var backHit = Physics.Raycast(
                new Ray(tile.transform.position + Vector3.up * 5f, Vector3.back),
                out rayCastBack,
                20
                );

            if(leftHit || rightHit || forwardHit || backHit)
            {
                return;
            }
            if (raycastHit.collider.gameObject?.layer == LayerMask.NameToLayer("Road"))
            {
                return;
            }
            
            randomNumber = Random.Range(0, 360);
            var randomBush = bushModels[Random.Range(0, bushModels.Count - 1)];

            randomBush.transform.Rotate(Vector3.up * randomNumber);
            Instantiate(randomBush, tile.transform);
        }
    }
}
