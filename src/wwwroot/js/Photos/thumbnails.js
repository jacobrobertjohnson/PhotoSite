window.thumbnails = (function(sizer, sidebar) {
    window.addEventListener("resize", render);

    function render() {
        let height = sizer && sizer.getHeight(),
            images = document.getElementsByClassName("photo-thumbnail"),
            $img,
            urlTemplate;

        for (let i = 0; i < images.length; i++) {
            $img = images[i];
            urlTemplate = $img.getAttribute("data-background-url");
            $img.style.backgroundImage = "url(" + urlTemplate.replace("[size]", height) + ")";
        }
    }

    return {
        render: render
    }
})(window.imageSizer, window.sidebar);