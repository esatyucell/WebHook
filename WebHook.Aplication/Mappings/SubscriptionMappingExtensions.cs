using WebHook.Domain.Entities;
using WebHook.Application.DTOs;

namespace WebHook.Application.Mappings
{
    public static class SubscriptionMappingExtensions
    {
        // 1. Kullanıcıdan gelen DTO'yu, veritabanı Entity'sine çevirir
        public static Subscription ToEntity(this SubscriptionCreateDto dto)
        {
            // Eğer gelen veri boşsa, hata fırlatmak yerine null dönüyoruz (Güvenlik kontrolü)
            if (dto == null)
                return null;

            // Object Initializer (Nesne İlklendirici) yapısıyla yeni nesneyi oluşturup alanları kopyalıyoruz
            return new Subscription
            {
                Name = dto.Name,
                EndpointUrl = dto.EndpointUrl,
                EventType = dto.EventType

                // DİKKAT: Id, CreatedAt, Status, SecretKey gibi alanları burada YAZMIYORUZ.
                // Çünkü bu nesne henüz "yeni doğdu" ve bu kimlik bilgilerini Service (İş mantığı) atayacak.
            };
        }

        // 2. Veritabanındaki Entity'yi, kullanıcıya döneceğimiz DTO'ya çevirir
        public static SubscriptionResponseDto ToResponseDto(this Subscription entity)
        {
            if (entity == null)
                return null;

            return new SubscriptionResponseDto
            {
                // Burada veritabanı modelindeki HER ŞEYİ DTO'ya kopyalıyoruz
                Id = entity.Id,
                Name = entity.Name,
                EndpointUrl = entity.EndpointUrl,
                EventType = entity.EventType,
                Status = entity.Status,
                SecretKey = entity.SecretKey,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}

// Yorum satırı ile yapılan açıklamalar Gemini tarafından oluşturulmuştur. 