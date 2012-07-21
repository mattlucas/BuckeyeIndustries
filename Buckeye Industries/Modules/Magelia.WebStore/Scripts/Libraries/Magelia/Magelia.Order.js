Magelia.Order = function (container) {
    this.container = container;
    this.initialize();
};

Magelia.Order.prototype = {
    container: null,

    initialize: function () {
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        var refreshViewProxy = $.proxy(this.refreshView, this);
        $('a', this.container).each(
            function () {
                var link = $(this);
                var href = link.attr('href');
                link.attr({ href: 'javascript:void(0);' }).click(function () { refreshViewProxy(href); });
            }
        );
    },

    refreshView: function (url) {
        $.ajax(
            {
                url: url,
                type: 'get',
                success: $.proxy(
                    function (html) {
                        var orders = $(html).insertAfter(this.container);
                        Magelia.Helpers.removeWrapper(orders);
                        this.container.remove();
                    },
                    this
                )
            }
        );
    }
};