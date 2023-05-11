window.sidebar = (function() {
    const PERSIST_ID = "showSidebar",
        PERSIST_VAL = localStorage.getItem(PERSIST_ID);

    let _showSidebar = PERSIST_VAL ? PERSIST_VAL === "true" : window.innerWidth > 480,
        _$toggler = document.getElementById("sidebar-toggler"),
        _subscriptions = [];

    _$toggler.addEventListener("click", toggleSidebar);
    document.body.addEventListener("transitionend", notify)

    update();

    function toggleSidebar() {
        _showSidebar = !_showSidebar;
        update();
    }

    function update() {
        render();
        persist();
    }

    function render() {
        if (_showSidebar)
            document.body.classList.add("has-sidebar");
        else
            document.body.classList.remove("has-sidebar");
    }

    function notify() {
        for (let i = 0; i < _subscriptions.length; i++) {
            _subscriptions[i]();
        }
    }

    function persist() {
        localStorage.setItem(PERSIST_ID, _showSidebar);
    }

    return {
        subscribe: function(action) {
            _subscriptions.push(action);
        }
    }
})();