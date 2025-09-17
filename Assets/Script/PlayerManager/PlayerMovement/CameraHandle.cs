using Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PlayerController
{
    public class CameraHandle : MonoBehaviour
    {
        InputHandle inputHandle;
        PlayerManager playerManager;
        public Transform targetTransform;
        public Transform cameraTransform;
        public Transform cameraPivotTransform;
        private Transform myTransform;
        private Vector3 cameraTransformPosition;
        public LayerMask ignoreLayers;
        public LayerMask environmentLayer;
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraHandle singleton;

        public float lookSpeed = 0.1f;
        public float followSpeed = 0.1f;
        public float pivotSpeed = 0.03f;

        private float targetPosition;
        public float defaultPosition;
        private float lookAngle;
        private float pivotAngle;
        public float minimumPivot = -35;
        public float maximumPivot = 35;
        public float rotationSpeed = 1;
        public float cameraSphereRadius = 0.2f;
        public float cameraCollisionOffset = 0.2f;
        public float minimumCollisionOffset = 0.2f;

        public float lockPivotPosition = 2.25f;
        public float unlockPivotPosition = 1.65f;

        List<CharacterManager> availableTargets = new List<CharacterManager>();
        public Transform nearestLockOnTarget;
        public Transform currentLockOnTarget;
        public Transform leftLockTarget;
        public Transform rightLockTarget;
        public float maximumLockOnDistance = 30;

#if UNITY_WEBGL && !UNITY_EDITOR
        float mouseSensitivity = 0.003f;
#else
        float mouseSensitivity = 0.01f; //editor / PC
#endif
        private void Awake()
        {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            targetTransform = FindObjectOfType<PlayerManager>().transform;
            inputHandle = FindObjectOfType<InputHandle>();
            playerManager = FindObjectOfType<PlayerManager>();
        }

        private void Start()
        {
            environmentLayer = 1 << LayerMask.NameToLayer("Environment");
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            myTransform.position = targetPosition;
            HandleCameraCollisions(delta);
        }

        public void ResetCameraVelocity()
        {
            cameraFollowVelocity = Vector3.zero;
        }

        public void ForceResetCamera(Vector3 newPosition)
        {
            // Reset position ngay lập tức
            myTransform.position = newPosition;

            // Reset velocity của SmoothDamp
            cameraFollowVelocity = Vector3.zero;

            // Force update target position để SmoothDamp không còn "nhớ" vị trí cũ
            // Trick: gọi SmoothDamp với deltaTime = 0 để reset internal state
            Vector3 dummyVelocity = Vector3.zero;
            Vector3.SmoothDamp(newPosition, newPosition, ref dummyVelocity, 0.01f);
            cameraFollowVelocity = Vector3.zero; // Reset lại sau trick
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            //Quaternion pivotTargetRotation = cameraPivotTransform.localRotation;
            if (inputHandle.lockOnFlag == false && currentLockOnTarget == null)
            {
                lookAngle += (mouseXInput * lookSpeed) * mouseSensitivity;
                pivotAngle -= (mouseYInput * pivotSpeed) * mouseSensitivity;

                pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

                Vector3 rotation = Vector3.zero;
                rotation.y = lookAngle;
                Quaternion targetRotation = Quaternion.Euler(rotation);
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                rotation = Vector3.zero;
                rotation.x = pivotAngle;

                targetRotation = Quaternion.Euler(rotation);
                cameraPivotTransform.localRotation = Quaternion.Slerp(cameraPivotTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {

                Vector3 direction = currentLockOnTarget.position - transform.position;
                direction.Normalize();
                direction.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;

                direction = currentLockOnTarget.position - cameraPivotTransform.position;
                direction.Normalize();

                targetRotation = Quaternion.LookRotation(direction);
                Vector3 eulerAngle = targetRotation.eulerAngles;
                eulerAngle.y = 0;
                //cameraPivotTransform.localEulerAngles = eulerAngle;
                cameraPivotTransform.localRotation = Quaternion.Slerp(cameraPivotTransform.localRotation, Quaternion.Euler(eulerAngle), rotationSpeed * Time.deltaTime);
            }


        }

        private void HandleCameraCollisions(float delta)
        {
            targetPosition = defaultPosition;
            RaycastHit hit;
            Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
            direction.Normalize();

            if (Physics.SphereCast
                (cameraPivotTransform.position, cameraSphereRadius, direction, out hit, Mathf.Abs(targetPosition), ignoreLayers))
            {
                float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
                targetPosition = -(dis - cameraCollisionOffset);
            }

            if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
            {
                targetPosition = -minimumCollisionOffset;
            }

            cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.15f);
            cameraTransform.localPosition = cameraTransformPosition;
        }

        public void HandleLockOn()
        {
            float shortestDistance = Mathf.Infinity;
            float shortestDistanceFromLeftTarget = Mathf.Infinity;
            float shortestDistanceFromRightTarget = Mathf.Infinity;

            Collider[] colliders = Physics.OverlapSphere(targetTransform.position, 26);

            for (int i = 0; i < colliders.Length; i++)
            {
                CharacterManager characterManager = colliders[i].GetComponent<CharacterManager>();

                if (characterManager != null)
                {
                    Vector3 lockTargetDirection = characterManager.transform.position - targetTransform.position;
                    float distanceFromTarget = lockTargetDirection.magnitude;
                    float viewableAngle = Vector3.Angle(lockTargetDirection, cameraTransform.forward);
                    RaycastHit hit;

                    if (characterManager.transform.root != targetTransform.transform.root
                        && viewableAngle > -50 && viewableAngle < 50
                        && distanceFromTarget <= maximumLockOnDistance)
                    {
                        if (Physics.Linecast(playerManager.lockOnTransform.position, characterManager.transform.position, out hit))
                        {
                            Debug.DrawLine(playerManager.lockOnTransform.position, characterManager.transform.position);

                            if (hit.transform.gameObject.layer == environmentLayer)
                            {
                                //cannot lockon

                            }
                            else
                            {
                                availableTargets.Add(characterManager);
                            }
                        }

                    }
                }
            }

            for (int i = 0; i < availableTargets.Count; i++)
            {
                float distanceFromTarget = Vector3.Distance(targetTransform.position, availableTargets[i].transform.position);

                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    nearestLockOnTarget = availableTargets[i].lockOnTransform;
                }

                if (inputHandle.lockOnFlag)
                {
                    Vector3 relativeEnemyPosition = currentLockOnTarget.InverseTransformPoint(availableTargets[i].transform.position);
                    var distanceFromLeftTarget = currentLockOnTarget.transform.position.x - availableTargets[i].transform.position.x;
                    var distanceFromRightTarget = currentLockOnTarget.transform.position.x + availableTargets[i].transform.position.x;

                    if (relativeEnemyPosition.x > 0.00 && distanceFromLeftTarget < shortestDistanceFromLeftTarget)
                    {
                        shortestDistanceFromLeftTarget = distanceFromLeftTarget;
                        leftLockTarget = availableTargets[i].lockOnTransform;
                    }

                    if (relativeEnemyPosition.x < 0.00 && distanceFromRightTarget < shortestDistanceFromRightTarget)
                    {
                        shortestDistanceFromRightTarget = distanceFromRightTarget;
                        rightLockTarget = availableTargets[i].lockOnTransform;
                    }
                }
            }
        }

        public void ClearLockOnTarget()
        {
            availableTargets.Clear();
            currentLockOnTarget = null;
            nearestLockOnTarget = null;
        }

        public void SetCameraHeight()
        {
            Vector3 velocity = Vector3.zero;
            Vector3 newLockedPosition = new Vector3(0, lockPivotPosition);
            Vector3 newUnlockedPosition = new Vector3(0, unlockPivotPosition, 0);

            if (currentLockOnTarget != null)
            {
                cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.localPosition, newLockedPosition, ref velocity, Time.deltaTime);
            }
            else
            {
                cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.localPosition, newUnlockedPosition, ref velocity, Time.deltaTime);
            }
        }
    }
}

