(function() {
    let _$photoList = document.getElementById("photo-list"),
        _$viewer = document.getElementById("photo-viewer"),
        _$closeButton = document.getElementById("close-photo-viewer"),
        _$prevButton = document.getElementById("prev-photo"),
        _$nextButton = document.getElementById("next-photo"),
        _$downloadButton = document.getElementById("photo-viewer-download-button");

        _$hideWhenOpen = [
            _$photoList,
            document.getElementById("photo-list-footer-left"),
            document.getElementById("photo-list-footer-right"),
            document.getElementById("photo-list-sidebar")
        ],
        
        _$showWhenOpen = [
            _$viewer,
            document.getElementById("photo-viewer-footer-left")
        ],

        _$thumbnail = null;

    _$photoList.addEventListener("click", function(e) {
        if (e.target.classList.contains("photo-thumbnail"))
            update(e.target);
    });

    _$closeButton.addEventListener("click", function() {
        update(null);
    });

    _$prevButton.addEventListener("click", function() {
        update(_$thumbnail.previousElementSibling);
    });

    _$nextButton.addEventListener("click", function() {
        update(_$thumbnail.nextElementSibling);
    });
    
    document.getElementById("photo-viewer-delete-button").addEventListener("click", function() {
        if (window.photoList.deletePhotos([_$thumbnail.id], false)) {
            _$thumbnail.parentElement.removeChild(_$thumbnail);
            update(null);
        }
    });

    render();

    function update($thumbnail) {
        _$thumbnail = $thumbnail;
        render();
    }

    function render() {
        let open = !!_$thumbnail;

        setVisibility(_$showWhenOpen, open);
        setVisibility(_$hideWhenOpen, !open);

        if (open) {
            let fullsizeUrl = _$thumbnail.getAttribute("data-fullsize-url");

            _$viewer.style["background-image"] = "url('" + fullsizeUrl + "')"
            _$downloadButton.setAttribute("href", fullsizeUrl + "?download=true");

            setVisibility([_$prevButton], _$thumbnail.previousElementSibling);
            setVisibility([_$nextButton], _$thumbnail.nextElementSibling);
        }
    }

    function setVisibility($els, visible) {
        for (let i = 0; i < $els.length; i++)
            $els[i].classList[visible ? "remove" : "add"]("hidden");
    }
})();