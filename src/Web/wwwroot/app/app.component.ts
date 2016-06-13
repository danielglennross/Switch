import { Component } from '@angular/core';
import { Features } from './features/feature';

@Component({
    selector: 'my-app',
    template: '<features></features>',
    directives: [Features]
})

export class AppComponent { }