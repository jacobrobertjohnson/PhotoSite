window.thumbnails = (function(containerSel, sizer, sidebar) {
    let _$container = document.querySelector(containerSel),
        _thumbnails = [];

    window.addEventListener("resize", render);

    function addPhotos(photos) {
        _thumbnails = photos;
    }

    function render() {
        _$container.innerHTML = "";

        for (let i = 0; i < 100 /*_thumbnails.length*/; i++) {
            renderThumbnail(_thumbnails[i]);
        }
    }

    function renderThumbnail(thumbInfo) {
        let $img = document.createElement("div"),
            height = sizer.getHeight();

        $img.id = thumbInfo.id;
        $img.classList.add("photo-thumbnail");
        $img.style.backgroundImage = "url(" + thumbInfo.thumbnailUrl.replace("[size]", height) + ")";
        $img.innerHTML = "&nbsp;";
        
        _$container.appendChild($img);
    }

    return {
        get: async function(familyId) {
            const response = await fetch("/Photos/" + familyId + "/Thumbnails"),
                photos = await response.json();

            addPhotos(photos);
            render();
        }
    }
})("#thumbnails", window.imageSizer, window.sidebar);