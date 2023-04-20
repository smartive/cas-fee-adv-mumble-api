using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MumbleApi.Database;

public class UlidConverter : ValueConverter<Ulid, string>
{
    public UlidConverter()
        : base(ulid => ulid.ToString(), @string => Ulid.Parse(@string))
    {
    }
}
