namespace JobApplication.Application.Common
{
    public static class Roles
    {
        public const string Candidate = "Candidate";
        public const string Recruiter = "Recruiter";

        // Name of the role claim inside the JWT (MapInboundClaims = false keeps it as "role").
        public const string ClaimType = "role";
    }
}
