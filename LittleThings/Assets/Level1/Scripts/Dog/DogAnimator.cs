using UnityEngine;

public class DogAnimator : MonoBehaviour
{
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private FootprintAnimator foorprintAnimator;
    [SerializeField] private Transform transform_FL;
    [SerializeField] private Transform transform_FR;
    [SerializeField] private Transform transform_RL;
    [SerializeField] private Transform transform_RR;

    public void Idle()
    {
        dogAnimator.SetTrigger("Idle");
    }
    public void Walk()
    {
        dogAnimator.SetTrigger("Walk");
    }
    public void Trot()
    {
        dogAnimator.SetTrigger("Trot");
    }
    public void Gallop()
    {
        dogAnimator.SetTrigger("Gallop");
    }
    public void Run()
    {
        dogAnimator.SetTrigger("Run");
    }

    public void FrontLeftFoot()
    {
        //Debug.Log("FrontLeftFoot");
        foorprintAnimator.AnimateFrontLeft(transform_FL.position, transform.rotation.eulerAngles);
    }
    public void FrontRightFoot()
    {
        //Debug.Log("FrontRightFoot");
        foorprintAnimator.AnimateFrontRight(transform_FR.position, transform.rotation.eulerAngles);
    }
    public void RearLeftFoot()
    {
        //Debug.Log("RearLeftFoot");
        foorprintAnimator.AnimateRearLeft(transform_RL.position, transform.rotation.eulerAngles);
    }
    public void RearRightFoot()
    {
        //Debug.Log("RearRightFoot");
        foorprintAnimator.AnimateRearRight(transform_RR.position, transform.rotation.eulerAngles);
    }
}
