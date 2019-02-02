using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGen : MonoBehaviour
{
    //holder of all generated objects
    public Transform forestParent;

    //vars for setting generation size
    public float dist;
    public int desiredAmount;
    public Vector3 finalPos;

    //prefab holders
    public List<GameObject> treePrefabs = new List<GameObject>();
    public List<GameObject> mushroomPrefabs = new List<GameObject>();

    //list of active objects
    public List<GameObject> allObjects = new List<GameObject>();
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> mushrooms = new List<GameObject>();

    
    void Awake()
    {
        GenerateTrees();

        forestParent.transform.position = finalPos;

        //set all shroom Y pos
        for(int i = 0; i < mushrooms.Count; i++)
        {
            mushrooms[i].GetComponent<SetShroom>().SetYPos();
        }
    }

    void GenerateTrees()
    {
        for (int i = 0, y = 0; y <= desiredAmount; y++)
        {
            for (int x = 0; x <= desiredAmount; x++, i++)
            {
                transform.position = new Vector3(x * dist, transform.position.y, y * dist);

                float randomChance = Random.Range(0, 100);

                //75% chance to spawn a tree
                if(randomChance < 75)
                {
                    //instantiate randomTree from prefab list
                    int randomTree = Random.Range(0, treePrefabs.Count);
                    GameObject treeClone = Instantiate(treePrefabs[randomTree], transform.position, Quaternion.identity, forestParent);

                    //randomize scale
                    float randomScale = Random.Range(0.75f, 2f);
                    treeClone.transform.localScale *= randomScale;

                    //randomize rotation
                    float randomRotation = Random.Range(0, 360);
                    treeClone.transform.localEulerAngles = new Vector3(0, randomRotation, 0);
                    
                    //add to lists
                    allObjects.Add(treeClone);
                    trees.Add(treeClone);

                    //find corresponding mushroom type to tree type
                    int treeIndex = treePrefabs.IndexOf(treePrefabs[randomTree]);

                    //adjust trees height
                    //RaycastHit hit;

                    //if (Physics.Raycast(treeClone.transform.position, Vector3.down, out hit, Mathf.Infinity, ground))
                    //{
                    //        treeClone.transform.position = hit.point;
                    //        Debug.Log("moved");
                    //treeClone.transform.position = new Vector3(transform.position.x, transform.position.y + (transform.localScale.y / 2), transform.position.z);

                    //}
                    GenerateShrooms(treeClone.transform, treeIndex, 5, 10);
                }
            }
        }
    }

    void GenerateShrooms(Transform treeParent, int shroomType, int mushroomMin, int mushroomMax)
    {
        int randomShroomCount = Random.Range(mushroomMin, mushroomMax);

        for(int i = 0; i < randomShroomCount; i++)
        {

            Vector3 spawnPos = treeParent.transform.position + Random.insideUnitSphere * 5;

            //check if player or house is in this gridSpot
            bool canGenerate = true;

            Collider[] hitColliders = Physics.OverlapSphere(spawnPos, 1);

            for (int h = 0; h < hitColliders.Length; h++)
            {
                if (hitColliders[h].gameObject.tag == "Tree" || hitColliders[h].gameObject.tag == "Player")
                {
                    canGenerate = false;
                }
            }

            //if no player/house, generate tree
            if (canGenerate)
            {
                //generate random tree type
                GameObject shroomClone = Instantiate(mushroomPrefabs[shroomType], spawnPos, Quaternion.identity, forestParent);
                mushrooms.Add(shroomClone);
                allObjects.Add(shroomClone);

            }
        }
    }

 
   
}
