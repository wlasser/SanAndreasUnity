using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleEnteringState : BaseVehicleState
	{
		Coroutine m_coroutine;


		public override void OnBecameInactive()
		{
			// restore everything

			if (!m_ped.IsInVehicle)
			{
				m_ped.characterController.enabled = true;
				// restore seat's occupying ped ? - no
				m_ped.transform.SetParent(null, true);
				m_model.IsInVehicle = false;
			}

			if (m_coroutine != null)
				StopCoroutine(m_coroutine);
			m_coroutine = null;

			base.OnBecameInactive();
		}

		public bool TryEnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			Net.NetStatus.ThrowIfNotOnServer();

			if (!this.CanEnterVehicle (vehicle, seatAlignment))
				return false;

			this.EnterVehicle(vehicle, seatAlignment, immediate);
			
			return true;
		}

		void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			
			Vehicle.Seat seat = vehicle.GetSeat (seatAlignment);

			// switch state here
			m_ped.SwitchState<VehicleEnteringState>();

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;
			seat.OccupyingPed = m_ped;

			m_ped.characterController.enabled = false;


			m_ped.transform.SetParent(seat.Parent);
			m_ped.transform.localPosition = Vector3.zero;
			m_ped.transform.localRotation = Quaternion.identity;

			if (seat.IsDriver)
			{
				// TODO: this should be done when ped enters the car
				vehicle.StartControlling();
			}

			m_model.IsInVehicle = true;


			if (!vehicle.IsNightToggled && WorldController.IsNight)
				vehicle.IsNightToggled = true;
			else if (vehicle.IsNightToggled && !WorldController.IsNight)
				vehicle.IsNightToggled = false;


			// send message to clients
			if (!immediate)
				Net.PedSync.Local.PedStartedEnteringVehicle();


			m_coroutine = StartCoroutine (EnterVehicleAnimation (seat, immediate));

		}

		private System.Collections.IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
		{
			var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

			m_model.VehicleParentOffset = Vector3.Scale(m_model.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = m_model.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				// TODO: also check if this state is still active state
				while (animState.enabled)
				{
					yield return new WaitForEndOfFrame();
				}
			}

			// TODO: check if this state is still active, and if vehicle is alive


			// ped now completely entered the vehicle

			// call method from VehicleSittingState - he will switch state
			m_ped.GetStateOrLogError<VehicleSittingState> ().EnterVehicle(this.CurrentVehicle, seat);

			// this variable is not needed - it can be obtained based on current state
		//	IsInVehicleSeat = true;


		}

		internal void PedStartedEnteringVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
		{
			// sent from server


		}

	}

}
