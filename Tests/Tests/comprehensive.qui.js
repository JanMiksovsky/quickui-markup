﻿/*
Comprehensive control:
Tests a bunch of compiler features.
*/
var Comprehensive = SampleBaseClass.sub({
    className: "Comprehensive",
    genericDefault: "true",
    tag: "div",
    inherited: {
        content: [
            " ",
            /* Say hello */
            " ",
            {
                control: "Simple",
                ref: "message",
                content: " Hello, <i>world</i>! "
            },
            " ",
            {
                html: "<div />",
                content: [
                    " ",
                    {
                        html: "<p />",
                        ref: "Comprehensive_content"
                    },
                    " "
                ]
            },
            " "
        ]
    }
});
Comprehensive.prototype.extend({
  content: Control.chain( "$Comprehensive_content", "content", ),
  foo: function()
  {
      alert("Hello, world!");
  }
});

