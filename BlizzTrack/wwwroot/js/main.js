$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip()

    $().ready(function () {
        $sidebar = $('.sidebar');
        $navbar = $('.navbar');
        $main_panel = $('.main-panel');

        $full_page = $('.full-page');

        $sidebar_responsive = $('body > .navbar-collapse');
        sidebar_mini_active = true;
        white_color = false;

        window_width = $(window).width();

        fixed_plugin_open = $('.sidebar .sidebar-wrapper .nav li.active a p').html();

        const data = window.store.get("config");
        window.localSettings = {};
        if (data)
            window.localSettings = JSON.parse(data);

        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            if (window.localSettings.theme != "device") {
                return;
            }
            const newColorScheme = e.matches ? "dark" : "light";

            if (newColorScheme === "dark") {
                $('body').addClass('change-background');
                setTimeout(function () {
                    $('body').removeClass('change-background');
                    $('body').removeClass('white-content');
                }, 900);

                white_color = false;
            } else {
                $('body').addClass('change-background');
                setTimeout(function () {
                    $('body').removeClass('change-background');
                    $('body').addClass('white-content');
                }, 900);

                white_color = true;
            }
        });

        if (window.localSettings.theme) {
            const newColorScheme = window.localSettings.theme;

            if (newColorScheme === "dark") {
                $('body').addClass('change-background');
                setTimeout(function () {
                    $('body').removeClass('change-background');
                    $('body').removeClass('white-content');
                }, 900);

                white_color = false;
            } else {
                $('body').addClass('change-background');
                setTimeout(function () {
                    $('body').removeClass('change-background');
                    $('body').addClass('white-content');
                }, 900);

                white_color = true;
            }
        }

        if (window.localSettings.mini_bar) {
            $("body").addClass("sidebar-mini");
        }

        if (window.localSettings.sidebar) {
            var new_color = window.localSettings.sidebar;

            if ($sidebar.length != 0) {
                $sidebar.attr('data', new_color);
            }

            if ($main_panel.length != 0) {
                $main_panel.attr('data', new_color);
            }

            if ($full_page.length != 0) {
                $full_page.attr('filter-color', new_color);
            }

            if ($sidebar_responsive.length != 0) {
                $sidebar_responsive.attr('data', new_color);
            }

            const $events = $(".fixed-plugin .background-color span");
            $events.siblings().removeClass('active');

            $("span.badge[data-color='" + new_color + "']").addClass("active");
        }

        $('.fixed-plugin a').click(function (event) {
            if ($(this).hasClass('switch-trigger')) {
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else if (window.event) {
                    window.event.cancelBubble = true;
                }
            }
        });

        $('.fixed-plugin .background-color span').click(function () {
            $(this).siblings().removeClass('active');
            $(this).addClass('active');

            var new_color = $(this).data('color');

            if ($sidebar.length != 0) {
                $sidebar.attr('data', new_color);
            }

            if ($main_panel.length != 0) {
                $main_panel.attr('data', new_color);
            }

            if ($full_page.length != 0) {
                $full_page.attr('filter-color', new_color);
            }

            if ($sidebar_responsive.length != 0) {
                $sidebar_responsive.attr('data', new_color);
            }

            window.localSettings.sidebar = new_color;
            window.store.set("config", JSON.stringify(window.localSettings));
        });

        $('.light-badge').click(function () {
            $('body').addClass('white-content');

            window.localSettings.theme = "light";
            window.store.set("config", JSON.stringify(window.localSettings));
        });

        $('.dark-badge').click(function () {
            $('body').removeClass('white-content');

            window.localSettings.theme = "dark";
            window.store.set("config", JSON.stringify(window.localSettings));
        });
    });
});
