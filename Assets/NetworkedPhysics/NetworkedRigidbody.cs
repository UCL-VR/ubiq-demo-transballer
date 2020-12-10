﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.Messaging;
using Ubik.Samples;
using Ubik.XR;


namespace Transballer.NetworkedPhysics
{
    public class NetworkedRigidbody : NetworkedObject, IGraspable
    {
        public UnityEngine.Rigidbody rb;
        RigidbodyManager manager;

        public float velSquareMag { get { return rb.velocity.sqrMagnitude; } }

        public Hand graspingController;
        public bool graspedRemotely = false;

        override protected void Awake()
        {
            base.Awake();
            manager = GameObject.FindObjectOfType<RigidbodyManager>();
            rb = GetComponent<UnityEngine.Rigidbody>();
        }

        override public void OnSpawned(bool local)
        {
            base.OnSpawned(local);
            manager.Register(this);
        }

        override public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            string msgString = message.ToString();
            string messageType = Messages.GetType(msgString);

            switch (messageType)
            {
                case "rigidbodyUpdate":
                    Messages.RigidbodyUpdate rbUpdate = Messages.RigidbodyUpdate.Deserialize(msgString);
                    OnRbUpdate(rbUpdate);
                    break;
                case "graspUpdate":
                    Messages.GraspUpdate graspUpdate = Messages.GraspUpdate.Deserialize(msgString);
                    OnGraspOrRelease(graspUpdate.grasped);
                    break;
                case "setKinematic":
                    Messages.SetKinematic setKinematic = Messages.SetKinematic.Deserialize(msgString);
                    OnSetKinematic(setKinematic.state);
                    break;
                default:
                    base.ProcessMessage(message);
                    break;
            }
        }

        override public void TakeControl()
        {
            if (!graspedRemotely)
            {
                owner = true;
                ctx.Send(new Messages.NewOwner().Serialize());
            }
        }

        public override void Remove()
        {
            manager.Unregister(this);
            base.Remove();
        }

        override protected void OnRemove()
        {
            manager.Unregister(this);
            base.OnRemove();
        }

        public void SendUpdate()
        {
            // sends position and rigidbody update
            base.Move();
            if (owner)
            {
                ctx.Send(new Messages.RigidbodyUpdate(rb).Serialize());
            }
        }

        protected virtual void OnRbUpdate(Messages.RigidbodyUpdate update)
        {
            if (owner)
            {
                throw new System.Exception("received rigidbody update for locally controlled gameobject");
            }
            rb.velocity = update.linearVelocity;
            rb.angularVelocity = update.angularVelocity;
        }

        public virtual void Grasp(Hand controller)
        {
            // this client has grasped this object
            // become locally controlled and disable gravity
            owner = true;
            graspingController = controller;
            rb.useGravity = false;
            ctx.Send(new Messages.GraspUpdate(true).Serialize());
        }

        public void Release(Hand controller)
        {
            if (graspingController)
            {
                graspingController = null;
                rb.useGravity = true;
                ctx.Send(new Messages.GraspUpdate(false).Serialize());
            }
        }

        protected virtual void OnGraspOrRelease(bool grasped)
        {
            owner = false;
            graspingController = null;
            // disable gravity when picked up
            rb.useGravity = !grasped;
            graspedRemotely = grasped;
        }

        public void SetKinematic(bool state)
        {
            if (owner)
            {
                rb.isKinematic = state;
                ctx.Send(new Messages.SetKinematic(state).Serialize());
            }
        }

        protected virtual void OnSetKinematic(bool state)
        {
            if (owner)
            {
                throw new System.Exception("received setKinematic update for locally controlled gameobject");
            }
            rb.isKinematic = state;
        }

        protected virtual void FixedUpdate()
        {
            if (graspingController != null)
            {
                rb.angularVelocity *= 0;
                rb.velocity = (graspingController.transform.position - transform.position) * 20;
            }
        }
    }
}