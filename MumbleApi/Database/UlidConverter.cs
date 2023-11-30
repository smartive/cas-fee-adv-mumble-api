using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MumbleApi.Database;

public class UlidConverter() : ValueConverter<Ulid, string>(ulid => ulid.ToString() ?? string.Empty,
    @string => Ulid.Parse(@string));
