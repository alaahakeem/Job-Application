namespace JobApplication.Application.Interfaces
{
    // Implemented in the API project (needs the hosting environment), so Application stays free of ASP.NET types.
    public interface ICvStorage
    {
        /// <summary>Saves the CV and returns the stored relative path (goes into JobCandidateApplication.CvUrl).</summary>
        Task<string> SaveAsync(Stream content, string extension);
        void Delete(string cvUrl);
    }
}
