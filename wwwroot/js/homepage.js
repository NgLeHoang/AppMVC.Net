$(document).ready(function() {
    // Active slide when click next
    let index = 0;
    function next() {
        $(".car-container").eq(index).removeClass("active");
        index = (index + 1) % $(".car-container").length;
        $(".car-container").eq(index).addClass("active");
    };
    setInterval(next, 10000);

    $("#next").on("click", function() {
        $(".car-container").eq(index).removeClass("active");
        index = (index + 1) % $(".car-container").length;
        $(".car-container").eq(index).addClass("active");
    });

    // Active slide when click prev
    $("#prev").on("click", function() {
        $(".car-container").eq(index).removeClass("active");
        index = (index - 1 + $(".car-container").length) % $(".car-container").length;
        $(".car-container").eq(index).addClass("active");
    })
})
