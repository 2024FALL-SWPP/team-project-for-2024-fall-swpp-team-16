using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject tree;
    private TreeManager treeManager;
    
    // Start is called before the first frame update
    void Start()
    {
        treeManager = tree.GetComponent<TreeManager>();
        if (!treeManager)
        {
            Debug.LogError("TreeManager is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            treeManager.Collapse();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            treeManager.Grow();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            treeManager.Shrink();
        }
    }
}
