namespace Dodoco.Core.Network.Api.Company {

    public abstract class CompanyApi {

        public CompanyApiReturnCode retcode { get; set; } = CompanyApiReturnCode.ERROR;
        public string? message { get; set; }

        public bool IsSuccessfull() {

            return this.retcode == CompanyApiReturnCode.SUCCESS;

        }

    }

}