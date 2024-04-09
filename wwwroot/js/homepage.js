$(document).ready(function() {
    // Active slide when click next
    let index = 0;
    function next() {
        $(".image-container").eq(index).removeClass("active");
        index = (index + 1) % $(".image-container").length;
        $(".image-container").eq(index).addClass("active");
    };
    setInterval(next, 10000);

    $("#next").on("click", function() {
        $(".image-container").eq(index).removeClass("active");
        index = (index + 1) % $(".image-container").length;
        $(".image-container").eq(index).addClass("active");
    });

    // Active slide when click prev
    $("#prev").on("click", function() {
        $(".image-container").eq(index).removeClass("active");
        index = (index - 1 + $(".image-container").length) % $(".image-container").length;
        $(".image-container").eq(index).addClass("active");
    })
})