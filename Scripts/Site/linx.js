/*!
 * Code licensed under the Apache License v2.0.
 * For details, see http://www.apache.org/licenses/LICENSE-2.0.
 */

// jQuery for page scrolling feature - requires jQuery Easing plugin
$(function() {
    $('.page-scroll a').bind('click', function(event) {
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: $($anchor.attr('href')).offset().top
        }, 1500, 'easeInOutExpo');
        event.preventDefault();
    });
});

// Highlight the top nav as scrolling occurs
$('body').scrollspy({
    target: '.navbar-fixed-top'
})

// Closes the Responsive Menu on Menu Item Click
$('.navbar-collapse ul li a').click(function() {
    $('.navbar-toggle:visible').click();
});

// Windows Tiles 

	$( document ).ready(function() {
    $(".tile").height($("#tile1").width());
    $(".win-carousel").height($("#tile1").width());
     $(".win-carousel .item").height($("#tile1").width());
     
    $(window).resize(function() {
    if(this.resizeTO) clearTimeout(this.resizeTO);
	this.resizeTO = setTimeout(function() {
		$(this).trigger('resizeEnd');
	}, 10);
    });
    
    $(window).bind('resizeEnd', function() {
    	$(".tile").height($("#tile1").width());
        $(".win-carousel").height($("#tile1").width());
        $(".win-carousel .item").height($("#tile1").width());
    });

});

// Magnifying glass

		
		$('.magnifier').click(function(){
			
			var  carouselImage = $('.zoomable img');
			
				if ($(this).text() === 'Image zoom') {
					 $(this).text('Turn off image zoom');
					 carouselImage.magnify();
				}
				else {
					$(this).text('Image zoom');
					carouselImage.parent('.magnify').off();
					carouselImage.parent('.magnify').css('cursor', 'default');
				}
		});
		
// Hide the magnifying glass when on the 360 tour 
		
		$('.list-inline').on('click', 'li', function(e){
			
 			$(this).hasClass('hidezoom') ? $('.zoom-control').css('visibility', 'hidden') : $('.zoom-control').css('visibility', 'visible');
			
		});


// Carousel thumbnails - highlight active
		
	$('.slider-thumbs li').click(function(){
		$(this).addClass('active').siblings().removeClass('active');
	});

//Footer display telephone number

	$('.fa-phone').click(function(){
		$('.call-us').toggleClass('numbershow');
	});