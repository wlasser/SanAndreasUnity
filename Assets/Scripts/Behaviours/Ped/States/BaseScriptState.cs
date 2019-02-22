using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all states that are scripts.
	/// </summary>
	public abstract class BaseScriptState : MonoBehaviour, IPedState
	{

		protected Ped m_ped;
		protected PedModel m_model { get { return m_ped.PlayerModel; } }
	//	protected StateMachine m_stateMachine;
		protected new Transform transform { get { return m_ped.transform; } }
		public bool IsActiveState { get { return m_ped.CurrentState == this; } }



		protected virtual void Awake ()
		{
			m_ped = this.GetComponentInParent<Ped> ();
		}

		public virtual void OnBecameActive ()
		{
			
		}

		public virtual void OnBecameInactive ()
		{
			
		}

		public virtual bool RepresentsState (System.Type type)
		{
			var myType = this.GetType ();
			return myType.Equals (type) || myType.IsSubclassOf (type);
		}

		public bool RepresentsState<T> () where T : IState
		{
			return this.RepresentsState (typeof(T));
		}

		public virtual void UpdateState() {

			// read input

			// call appropriate function for every input action


			this.ConstrainPosition();
			this.ConstrainRotation();

		}

		public virtual void LateUpdateState()
		{

			if (m_ped.shouldPlayAnims)
				this.UpdateAnims ();
			
		}

		public virtual void FixedUpdateState()
		{

			this.UpdateHeading();
			this.UpdateRotation();
			this.UpdateMovement();

		}

		protected virtual void ConstrainPosition()
		{
			m_ped.ConstrainPosition();
		}

		protected virtual void ConstrainRotation ()
		{
			m_ped.ConstrainRotation();
		}

		protected virtual void UpdateHeading()
		{
			m_ped.UpdateHeading ();
		}

		protected virtual void UpdateRotation()
		{
			m_ped.UpdateRotation ();
		}

		protected virtual void UpdateMovement()
		{
			m_ped.UpdateMovement ();
		}

		protected virtual void UpdateAnims()
		{
			
		}


		public virtual void OnSubmitPressed() {

		}

		public virtual void OnJumpPressed() {

		}

		public virtual void OnFlyButtonPressed()
		{

		}

		public virtual void OnFlyThroughButtonPressed()
		{

		}

		public virtual void OnDamaged(DamageInfo info)
		{

		}

	}

}