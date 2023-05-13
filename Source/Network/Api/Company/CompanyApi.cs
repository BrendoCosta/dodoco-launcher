namespace Dodoco.Network.Api.Company {

    public abstract class CompanyApi {

        public int? retcode { get; set; }
        public string? message { get; set; }

        public bool IsSuccessfull() {

            return this.retcode == 0;

        }

    }

}