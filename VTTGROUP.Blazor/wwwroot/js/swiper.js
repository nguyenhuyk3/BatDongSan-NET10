window.initSwiper = (dotNetHelper) => {
    const modal = document.getElementById('PopupDetailSPModal');
    if (!modal) return;
    const setup = () => {
        setTimeout(() => {
            window.dotNetHelper = dotNetHelper;
            const thumbSwiper = new Swiper(".swiper-thumb", {
                spaceBetween: 10,
                slidesPerView: 7,
                freeMode: true,
                watchSlidesProgress: true,
            });

            const mainSwiper = new Swiper(".main-swiper", {
                spaceBetween: 10,
                navigation: {
                    nextEl: '.swiper-button-next',
                    prevEl: '.swiper-button-prev',
                },
                thumbs: { swiper: thumbSwiper },
            });

            mainSwiper.on('slideChange', function () {
                const dotNetHelper = window.dotNetHelper;
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('UpdateSlideIndex', mainSwiper.activeIndex + 1);
                    document.querySelectorAll('.main-swiper video').forEach(video => {
                        video.muted = true;
                        video.pause();
                    });
                    const activeSlide = mainSwiper.slides[mainSwiper.activeIndex];
                    const video = activeSlide.querySelector('video');
                }

                // if (video) {
                //   video.muted = false;
                //   video.play();
                // }
            });
        },100)
    };
    
    modal.addEventListener('shown.bs.modal', setup, { once: true });
};