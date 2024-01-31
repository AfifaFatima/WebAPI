using AutoMapper;
using WebAPI.Models.Database;
using WebAPI.Models.ViewModel;

namespace WebAPI.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            CreateMap<UploadDocument, UploadDocumentsVM>().ReverseMap();
        }
    }
}
