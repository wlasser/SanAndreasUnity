﻿using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class DriveByState : VehicleSittingState, IAimState
    {
        public float AimAnimMaxTime = 0f;
        public float AimAnimFireMaxTime = 0f;



        protected override void EnterVehicleInternal()
        {
            m_vehicleParentOffset = Vector3.zero;
            m_model.VehicleParentOffset = Vector3.zero;

			BaseVehicleState.PreparePedForVehicle(m_ped, this.CurrentVehicle, this.CurrentVehicleSeat);

            // we should not update firing from here, because it can cause stack overflow
            this.UpdateAnimsInternal(false);

        }

        protected override void UpdateAnimsInternal()
        {
            this.UpdateAnimsInternal(true);
        }

        void UpdateAnimsInternal(bool bUpdateFiring)
        {
            if (this.CurrentVehicleSeat != null)
            {
                var animId = new Importing.Animation.AnimId("drivebys", this.GetAnimBasedOnAimDir());
                m_model.PlayAnim(animId);
                m_model.LastAnimState.wrapMode = WrapMode.ClampForever;

                if (bUpdateFiring)
                {
                    if (m_ped.CurrentWeapon != null)
                    {
                        m_ped.CurrentWeapon.AimAnimState = m_model.LastAnimState;

                        this.UpdateAimAnim(() => BaseAimMovementState.TryFire(m_ped));
                    }
                }

                m_model.VehicleParentOffset = m_model.GetAnim(animId.AnimName).RootEnd;
                m_model.RootFrame.transform.localPosition = Vector3.zero;
            }
        }

        string GetAnimBasedOnAimDir()
        {
            // 4 types: forward, backward, same side, opposite side

            Vector3 aimDir = m_ped.AimDirection;
            Vector3 vehicleDir = this.CurrentVehicle.transform.forward;
            bool isLeftSeat = this.CurrentVehicleSeat.IsLeftHand;
            string leftOrRightLetter = isLeftSeat ? "L" : "R";

            float angle = Vector3.Angle(aimDir, vehicleDir);
            float rightAngle = Vector3.Angle(aimDir, this.CurrentVehicle.transform.right);

            if (angle < 45)
            {
                // aiming forward
                return "Gang_Driveby" + leftOrRightLetter + "HS_Fwd";
            }
            else if (angle < 135)
            {
                // aiming to left or right side
                bool isAimingToLeftSide = rightAngle > 90;
                if (isLeftSeat != isAimingToLeftSide)   // aiming to opposite side
                {
                    return "Gang_DrivebyTop_" + leftOrRightLetter + "HS";
                }
                else    // aiming to same side
                {
                    return "Gang_Driveby" + leftOrRightLetter + "HS";
                }
            }
            else
            {
                // aiming backward
                return "Gang_Driveby" + leftOrRightLetter + "HS_Bwd";
            }

        }

        void UpdateAimAnim(System.Func<bool> tryFireFunc)
        {
            var ped = m_ped;
            var weapon = ped.CurrentWeapon;
            var state = m_model.LastAnimState;
            float aimAnimMaxTime = this.AimAnimMaxTime;

            if (state.time >= aimAnimMaxTime)
            {
                // keep the anim at max time
                state.time = aimAnimMaxTime;
                ped.AnimComponent.Sample();
                state.enabled = false;

                if (ped.IsFiring)
                {
                    // check if weapon finished firing
                    if (weapon != null && weapon.TimeSinceFired >= (weapon.AimAnimFireMaxTime - weapon.AimAnimMaxTime))
                    {
                        if (Net.NetStatus.IsServer)
                        {
                            ped.StopFiring();
                        }
                    }
                }
                else
                {
                    // check if we should start firing

                    if (ped.IsFireOn && tryFireFunc())
                    {
                        // we started firing

                    }
                    else
                    {
                        // we should remain in aim state
                        
                    }
                }

            }

        }

        public virtual void StartFiring()
        {
            // switch to firing state
            m_ped.GetStateOrLogError<DriveByFireState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
        }

        // camera

        public override void OnAimButtonPressed()
        {
            // switch to sitting state
            if (m_isServer)
                m_ped.GetStateOrLogError<VehicleSittingState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
            else
                base.OnAimButtonPressed();
        }

    }

}