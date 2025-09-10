

namespace PassingCar.Models.API.User
{
    public class GetReviewsResponse : ApiBaseResponse
    {
        public IEnumerable<ReviewM> Reviews { get; set; }
    }
    public class ReviewM
    {
        public byte[] ProfilePhoto { get; set; }
        public string Name { get; set; }
        public double Rating { get; set; }
        public string Message { get; set; }
    }
    public class ReviewMExtended
    {
        public ImageSource UserProfilePhotoImg { get; set; }
        public string UserName { get; set; }
        public ImageSource UserStar1 { get; set; }
        public ImageSource UserStar2 { get; set; }
        public ImageSource UserStar3 { get; set; }
        public ImageSource UserStar4 { get; set; }
        public ImageSource UserStar5 { get; set; }
        public double Rating { get; set; }
        public string Message { get; set; }
        public ReviewMExtended(ReviewM reviewM)
        {
            UserName = string.IsNullOrEmpty(reviewM.Name) ? string.Empty : reviewM.Name.Split(' ')[0];
            UserProfilePhotoImg = reviewM.ProfilePhoto != null && reviewM.ProfilePhoto.Length > 0
                ? ImageSource.FromStream(() => new MemoryStream(reviewM.ProfilePhoto))
                : "img_account.png";
            Rating = reviewM.Rating;
            Message = reviewM.Message;
            UserStar1 = GetStarImage(1);
            UserStar2 = GetStarImage(2);
            UserStar3 = GetStarImage(3);
            UserStar4 = GetStarImage(4);
            UserStar5 = GetStarImage(5);
        }
        private ImageSource GetStarImage(int countStar)
        {
            return !(Rating > 0)
                ? null
                : (Rating - countStar) >= 0
                    ? "a_star_filled.png"
                    : (Rating - countStar) >= -0.5
                                    ? "Images.a_star_half.png"
                                    : "Images.a_star.png";
        }
    }
}
