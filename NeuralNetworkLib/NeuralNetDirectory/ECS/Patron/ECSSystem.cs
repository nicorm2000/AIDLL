namespace NeuralNetworkLib.NeuralNetDirectory.ECS.Patron
{
    public abstract class ECSSystem
    {
        /// <summary>
        /// Runs the system's logic by calling the PreExecute, Execute, and PostExecute methods sequentially.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame or tick.</param>
        public void Run(float deltaTime)
        {
            PreExecute(deltaTime);
            Execute(deltaTime);
            PostExecute(deltaTime);
        }

        /// <summary>
        /// Initializes the system. This method must be implemented by derived classes to set up any necessary system state or resources.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Deinitializes the system, cleaning up any resources or states. This method can be overridden by derived classes if custom deinitialization is needed.
        /// </summary>
        public virtual void Deinitialize()
        {
            
        }

        protected abstract void PreExecute(float deltaTime);

        protected abstract void Execute(float deltaTime);

        protected abstract void PostExecute(float deltaTime);
    }
}