using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Debex.Convert.Data;

namespace Debex.Convert.Environment
{
    public static class Mapper
    {
        private static readonly IMapper mapper;

        static Mapper()
        {
            mapper = new AutoMapper.Mapper(new MapperConfiguration(c =>
            {
                c.CreateMap<BaseField, BaseFieldToMatch>();
                c.CreateMap<BaseFieldToMatch, BaseFieldToMatch>();
                c.CreateMap<FileFieldToMatch, FileFieldToMatch>();
                c.CreateMap<CheckField, CheckFieldState>().ReverseMap();
                
                
                c.CreateMap<RegionMatchState, RegionMatch>().ReverseMap();
                c.CreateMap<CleanField, CleanFieldState>().ReverseMap();
                c.CreateMap<CalculatedField, CalculatedFieldState>().ReverseMap();

            }));
        }


        public static T Map<T>(this object val) => mapper.Map<T>(val);
    }
}
