using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerShipCharacteristics))]
public class PlayerShipController : ShipController
{
    public static event Action<float, float> OnCannonFired;

    [SerializeField] protected Vector2 _sensivity = new Vector2(10f, 100f);
    protected Transform _mainLeftTarget;
    protected Transform _mainRightTarget;

    [SerializeField] protected float[] _targetXLimits = new float[2];
    [SerializeField] protected float[] _targetZLimits = new float[2];

    [SerializeField] private GameObject _steeringWheel;

    [Serializable]
    private struct ScreenShakeParameters
    {
        public float Duration;
        public float Strength;
    }

    [SerializeField] private ScreenShakeParameters _screenShakeParameters;

    private PlayerShipCharacteristics _playerShipCharacteristics;
    private bool _canMoveForward = true;

    public void MoveForward()
    {
        if (_canMoveForward)
        {
            Vector3 forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);
            _rb.AddForce(forward * _playerShipCharacteristics.Speed * Time.deltaTime, ForceMode.Acceleration);
            _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _playerShipCharacteristics.MaxSpeed);
        }
    }

    public void Rotate(float rotationSide)
    {
        Vector3 side = -_steeringWheel.transform.right * rotationSide;
        _rb.AddForceAtPosition(side * Time.deltaTime * _playerShipCharacteristics.RotationSpeed, _steeringWheel.transform.position, ForceMode.Acceleration);
    }

    public virtual void AimLeftCannons(Vector2 mouseInput)
    {
        mouseInput.y = -mouseInput.y;
        AimCannons(mouseInput, _mainLeftTarget, _leftCannons);
    }

    public virtual void AimRightCannons(Vector2 mouseInput)
    {
        mouseInput.x = -mouseInput.x;
        AimCannons(mouseInput, _mainRightTarget, _rightCannons);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _playerShipCharacteristics = GetComponent<PlayerShipCharacteristics>();
        base._shipCharacteristics = _playerShipCharacteristics;
        _rb = GetComponent<Rigidbody>();
        _steeringWheel.transform.localPosition = new Vector3(_rb.centerOfMass.x, _rb.centerOfMass.y, _steeringWheel.transform.localPosition.z);
        GameObject.Find("Player").GetComponent<PlayerOnShipInputHandler>().SetShipController(this);


        _mainLeftTarget = transform.Find("MainLeftTarget");
        _mainRightTarget = transform.Find("MainRightTarget");

        AssignTargetToCannons();

    }

    protected override IEnumerator ShootWithCannons(List<Cannon> cannons)
    {
        foreach (Cannon cannon in cannons)
        {
            if (cannon.Loaded())
            {
                cannon.Shoot();
                OnCannonFired?.Invoke(_screenShakeParameters.Duration, _screenShakeParameters.Strength);
                yield return new WaitForSeconds(base._delayBetweenShotsInSeconds);
            }
            else
                yield break;
        }
    }

    protected void AimCannons(Vector2 mouseInput, Transform target, List<Cannon> cannons)
    {
        float newX = target.localPosition.x + (mouseInput.y * _sensivity.y * Time.deltaTime);
        if (target.localPosition.x > 0)
        {
            newX = Mathf.Clamp(newX, _targetXLimits[0], _targetXLimits[1]);
        }
        else
        {
            newX = Mathf.Clamp(newX, -_targetXLimits[1], -_targetXLimits[0]);
        }
        float newZ = Mathf.Clamp(target.localPosition.z + (mouseInput.x * _sensivity.x * Time.deltaTime), _targetZLimits[0], _targetZLimits[1]);
        target.localPosition = new Vector3(newX, target.localPosition.y, newZ);
        target.position = new Vector3(target.position.x, 0, target.position.z);

        foreach (Cannon cannon in cannons)
            cannon.Aim();
    }

    private void AssignTargetToCannons()
    {
        foreach (Cannon cannon in _leftCannons)
            cannon.SetTarget(_mainLeftTarget);
        foreach (Cannon cannon in _rightCannons)
            cannon.SetTarget(_mainRightTarget);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            _canMoveForward = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Terrain"))
            _canMoveForward = true;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("Terrain"))
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.red);
                _rb.AddForceAtPosition(contact.normal * 5f, contact.point, ForceMode.Acceleration);
            }
    }
}
