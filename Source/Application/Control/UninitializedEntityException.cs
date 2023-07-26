namespace Dodoco.Application.Control {

    public class UninitializedEntityException: Exception {

        public UninitializedEntityException(): base("The target entity is not initialized") {}

    }

}