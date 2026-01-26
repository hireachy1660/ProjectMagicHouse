using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using UnityEngine;

public class myHandManager : MonoBehaviour
{
    private IInteractable currentHeldItem = null;
    private bool isGrabbed = false;
    [SerializeField]
    private RayInteractor myHandrayInterctor = null;
    
    public void OnSelect()
    {
        isGrabbed=true;
    }

    public void UnSelect()
    {
        isGrabbed=false;
    }

    private void Update()
    {
        

    }

    private void shootRay()
    {
        //if (isGrabbed && currentHeldItem. == "Bullet")
        //{
        //    var hit = myHandrayInterctor.Candidate;
        //    IMyIntertor target;
        //    if(hit.gameObject.TryGetComponent<IMyIntertor>(out target))
        //    {
        //        //target.OnRayHit()
        //    }
        //}
    }
}
