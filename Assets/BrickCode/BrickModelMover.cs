using System.Collections.Generic;
using UnityEngine;
using ModelCode;

namespace BrickCode
{
    /**
     * Handles moving/rotating bricks/brick structures and handles collisions sliding.
     */
    public class BrickModelMover : MonoBehaviour
    {
        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask = ~0; //Controls which layers are checked for collision, set to all.
        [SerializeField] private int collisionResolveIterations = 6; //Controls how many times the mover tries to push/pull objects out of collisions.
        [SerializeField] private float collisionSkin = 0.001f; //Minimum distance added when avoiding collisions.

        private BrickObjectData _brickData; //BrickObjectData the component is attached to.

        private readonly HashSet<Collider> _ownColliders = new(); //Stores colliders attached to current selected brick.
        private readonly List<BrickObjectData> _movingModelObjects = new(); //Stores all bricks currently being moved.
        private readonly HashSet<Collider> _movingModelColliders = new(); //Stores all colliders belonging to the moving model.

        public LayerMask CollisionMask => collisionMask;

        /**
         * Runs on creation.
         * Gets brick data attached to current brick and stores its own colliders.
         */
        private void Awake()
        {
            _brickData = GetComponent<BrickObjectData>();
            CacheOwnColliders();
        }

        
        /**
         * Decides which group of bricks should move.
         */
        public void PrepareMovingModel(BrickObjectData selectedBrick)
        {
            _brickData = selectedBrick;
            ClearMovingModel();
            
            //if there is an active selection, move the selected objects
            if (BrickSelectionManager.Instance && BrickSelectionManager.Instance.IsSelected(_brickData)) 
            {
                List<BrickObjectData> selectedObjects = BrickSelectionManager.Instance.GetSelectedObjects();
                foreach (BrickObjectData selectedObject in selectedObjects)
                {
                    AddMovingObject(selectedObject);
                }
            }
            
            //if there is no current selection, find all the connected objects
            if (_movingModelObjects.Count == 0)
            {
                if (_brickData && _brickData.Brick != null)
                {
                    List<BrickObjectData> connectedObjects = BrickModelRegistry.GetConnectedObjects(_brickData.Brick);
                    foreach (BrickObjectData connectedObject in connectedObjects)
                    {
                        AddMovingObject(connectedObject);
                    }
                }
            }
            
            if (_movingModelObjects.Count == 0 && _brickData != null) AddMovingObject(_brickData); //if no selected group or connected model exists, just move this brick
            CacheMovingModelColliders(); //collect all the collision for selected bricks
        }

        /**
         * Resets current moving group.
         */
        public void ClearMovingModel()
        {
            _movingModelObjects.Clear();
            _movingModelColliders.Clear();
        }

        /**
         * Checks if a given brick is part of the currently moving model.
         * Used to stop snap system from attempting to snap already connected pieces.
         */
        public bool IsMovingObject(BrickObjectData brick)
        {
            return brick && _movingModelObjects.Contains(brick);
        }

        /**
         * Tries to move active model to a target position while resolving collisions.
         * Returns true if movement is successful, false if movement failed and was reverted.
         */
        public bool MoveModelWithSlidingCollision(Vector3 selectedBrickTargetPosition, Quaternion selectedBrickTargetRotation)
        {
            //if only moving one brick use single brick movement method
            if (_movingModelObjects.Count <= 1) 
            {
                return MoveSingleBrickWithSlidingCollision(selectedBrickTargetPosition, selectedBrickTargetRotation);
            }
            
            //save old position and rotation
            Dictionary<BrickObjectData, Vector3> oldPositions = new(); 
            Dictionary<BrickObjectData, Quaternion> oldRotations = new();
            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (!obj) continue;
                oldPositions[obj] = obj.transform.position;
                oldRotations[obj] = obj.transform.rotation;
            }

            Vector3 selectedMoveDelta = selectedBrickTargetPosition - transform.position; //calculate move delta (how far to move)
            
            //move every brick in the model
            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (!obj) continue;
                obj.transform.position += selectedMoveDelta;
            }

            Physics.SyncTransforms(); //update Unity physics representation

            //try to push model out of anything it overlaps, if fail restore to old positions
            bool resolved = ResolveOverlapsByPushingOut();
            if (!resolved)
            {
                RestoreObjects(oldPositions, oldRotations);
                return false;
            }
            return true;
        }

        /**
         * Rotate selected brick/brick structure.
         * Returns true if successful, false if failed and restored
         */
        public bool RotateModelWithSlidingCollision(Quaternion selectedTargetRotation)
        {
            if (_movingModelObjects.Count <= 1) return MoveSingleBrickWithSlidingCollision(transform.position, selectedTargetRotation); //if only single brick selected, use single brick rotation method

            //save old positions/rotations
            Dictionary<BrickObjectData, Vector3> oldPositions = new();
            Dictionary<BrickObjectData, Quaternion> oldRotations = new();
            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (!obj) continue;

                oldPositions[obj] = obj.transform.position;
                oldRotations[obj] = obj.transform.rotation;
            }

            Quaternion rotationDelta = selectedTargetRotation * Quaternion.Inverse(transform.rotation); //calculate rotation delta
            Vector3 pivot = transform.position; //selected brick is pivot point

            //rotate every brick around pivot point
            foreach (BrickObjectData obj in _movingModelObjects)
            {
                if (!obj) continue;
                Vector3 relativePosition = obj.transform.position - pivot;
                obj.transform.position = pivot + rotationDelta * relativePosition;
                obj.transform.rotation = rotationDelta * obj.transform.rotation;
            }

            Physics.SyncTransforms(); //update Unity physics representation

            //attempt collision resolution
            bool resolved = ResolveOverlapsByPushingOut();
            if (!resolved)
            {
                RestoreObjects(oldPositions, oldRotations);
                return false;
            }
            return true;
        }

        /**
         * Handle movement for a single brick.
         * Returns true if successful, false if failed and restored
         */
        private bool MoveSingleBrickWithSlidingCollision(Vector3 targetPosition, Quaternion targetRotation)
        {
            //save old position
            Vector3 oldPosition = transform.position;
            Quaternion oldRotation = transform.rotation;

            transform.SetPositionAndRotation(targetPosition, targetRotation); //move brick
            Physics.SyncTransforms(); //update Unity physics representation

            //attempt collision resolution
            bool resolved = ResolveOverlapsByPushingOut();
            if (!resolved)
            {
                transform.SetPositionAndRotation(oldPosition, oldRotation);
                Physics.SyncTransforms();
                return false;
            }

            return true;
        }

        /**
         * Attempt to resolve collision by pushing the moving brick out of other bricks.
         * Returns true if is no longer overlapping other bricks.
         */
        private bool ResolveOverlapsByPushingOut()
        {
            for (int i = 0; i < collisionResolveIterations; i++) //collision resolution may require multiple passes
            {
                Vector3 totalCorrection = Vector3.zero; //total push direction for this iteration
                int correctionCount = 0; //number of overlaps
                foreach (Collider ownCollider in GetActiveMovingColliders()) //loop through all colliders on moving object
                {
                    if (!ownCollider || !ownCollider.enabled) continue;

                    Collider[] nearbyColliders = Physics.OverlapBox( //collect colliders near/overlapping moving collider's bounds
                        ownCollider.bounds.center,
                        ownCollider.bounds.extents,
                        Quaternion.identity,
                        collisionMask,
                        QueryTriggerInteraction.Ignore
                    );

                    foreach (Collider otherCollider in nearbyColliders) //loop through all colliders near the moving object
                    {
                        if (!IsValidCollisionTarget(otherCollider)) continue; //skip invalid colliders (not needed for checking)
                        bool overlapping = Physics.ComputePenetration( //check if colliders are physically overlapping and output direction and distance to resolve overlap
                            ownCollider,
                            ownCollider.transform.position,
                            ownCollider.transform.rotation,
                            otherCollider,
                            otherCollider.transform.position,
                            otherCollider.transform.rotation,
                            out Vector3 direction,
                            out float distance
                        );
                        if (!overlapping) continue; //skip collider if it isn't actually overlapping
                        totalCorrection += direction * (distance + collisionSkin); //calculate vector to resolve collision
                        correctionCount++; //increment number of found overlaps
                    }
                }

                if (correctionCount == 0) return true; //if no collisions/overlaps detected, return true (collisions resolved)
                MoveActiveModelBy(totalCorrection); //move the active model by calculated resolution vector
                Physics.SyncTransforms(); //update Unity physics representation
            }
            return !IsOverlappingOtherObjects(); //return true if no longer overlapping other objects
        }

        /**
         * Check if any moving collider is overlapping another object.
         * Return true if so.
         */
        private bool IsOverlappingOtherObjects()
        {
            foreach (Collider ownCollider in GetActiveMovingColliders())
            {
                if (!ownCollider || !ownCollider.enabled) continue; //skip invalid colliders
                Collider[] nearbyColliders = Physics.OverlapBox( //collect nearby colliders
                    ownCollider.bounds.center,
                    ownCollider.bounds.extents,
                    Quaternion.identity,
                    collisionMask,
                    QueryTriggerInteraction.Ignore
                );
                foreach (Collider otherCollider in nearbyColliders) //iterate through each nearby collider
                {
                    if (!IsValidCollisionTarget(otherCollider)) continue; //skip invalid colliders
                    bool overlapping = Physics.ComputePenetration( //return true if overlapping, ignore output
                        ownCollider,
                        ownCollider.transform.position,
                        ownCollider.transform.rotation,
                        otherCollider,
                        otherCollider.transform.position,
                        otherCollider.transform.rotation,
                        out _,
                        out _
                    );
                    if (overlapping) return true; //return true if any nearby collider is overlapping
                }
            }
            return false; //reach this return if no colliders overlap
        }

        /**
         * Checks if a collision target is a valid target.
         * A valid target must:
         *  - Not be null
         *  - Be enabled
         *  - Not be part of the moving model
         *  - Be a brick collider
         * Returns true if the collider meets valid conditions.
         */
        private bool IsValidCollisionTarget(Collider otherCollider)
        {
            bool hasValidCollider = !otherCollider && otherCollider.enabled;
            bool isNotPartOfMovingModel = !_ownColliders.Contains(otherCollider) && !_movingModelColliders.Contains(otherCollider);
            bool isBrick = otherCollider.GetComponentInParent<DraggableBrick3D>(); 
            return hasValidCollider && isNotPartOfMovingModel && isBrick;
        }

        /**
         * Decides which colliders are currently moving.
         * Returns either the full model or single brick colliders.
         */
        private IEnumerable<Collider> GetActiveMovingColliders()
        {
            if (_movingModelColliders.Count > 0) return _movingModelColliders;
            return _ownColliders;
        }

        /**
         * Applies a correction movement.
         * Used by collision resolution.
         */
        private void MoveActiveModelBy(Vector3 correction)
        {
            //if moving full model apply correction to each brick.
            if (_movingModelObjects.Count > 0)
            {
                foreach (BrickObjectData obj in _movingModelObjects)
                {
                    if (!obj) continue;
                    obj.transform.position += correction;
                }
                return;
            }

            transform.position += correction; //move single brick
        }

        /**
         * Returns objects to stored positions/rotations.
         */
        private void RestoreObjects(Dictionary<BrickObjectData, Vector3> oldPositions, Dictionary<BrickObjectData, Quaternion> oldRotations)
        {
            foreach (BrickObjectData obj in oldPositions.Keys)
            {
                if (!obj) continue;
                obj.transform.SetPositionAndRotation(oldPositions[obj], oldRotations[obj]);
            }
            Physics.SyncTransforms();
        }

        /**
         * Add a brick to _movingModelObjects.
         */
        private void AddMovingObject(BrickObjectData obj)
        {
            if (!obj) return;
            if (_movingModelObjects.Contains(obj)) return;
            _movingModelObjects.Add(obj);
        }

        /**
         * Store all colliders attached to given brick and its children.
         */
        private void CacheOwnColliders()
        {
            _ownColliders.Clear();
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider) _ownColliders.Add(collider);
            }
        }

        /**
         * Store all colliders attached to moving model.
         */
        private void CacheMovingModelColliders()
        {
            _movingModelColliders.Clear();
            foreach (BrickObjectData movingObject in _movingModelObjects)
            {
                if (!movingObject) continue;
                Collider[] colliders = movingObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    if (collider) _movingModelColliders.Add(collider);
                }
            }
        }
    }
}