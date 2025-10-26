using UnityEngine;
/// <summary>
/// 用于平面点击直接移动过去
/// </summary>
public class MainPlayerMoveTest : MonoBehaviour
{
    public bool isCanMove = true;
    public NotMainPlayerMove notMainPlayerMove;

    Vector3 vector3 = Vector3.zero;
    GameObject go = null;
    private void Start()
    {
        notMainPlayerMove.OnSetData(this.GetComponent<MainPlayerMove>());
    }
    // Update is called once per frame
    void Update()
    {
        if (isCanMove && Input.GetMouseButton(0))
        {
            Tools.OnBackHitPointAndGameObject(Input.mousePosition,ref vector3, ref go, "MainPlane");
            if (go != null)
            {
                transform.position = vector3;
            }
        }
    }
}
