import { Component, OnInit } from '@angular/core';
import { Feature, FeatureSet, FeatureStatus } from './models'
import { FeatureService } from './featureService';
import { FeatureItem } from './featureItem';

@Component({
    selector: 'features',   
    template: `
                <div id='features'>
                    <div *ngFor='let feature of features'>
                        <featureItem [feature]='feature'></featureItem>
                    </div>
                </div>
            `,
    directives: [FeatureItem],
    providers: [FeatureService]
})

export class Features implements OnInit {
    constructor(private _featureService: FeatureService) { }

    errorMessage: string;
    features: Feature[];

    ngOnInit(): void {
        this.populateFeatures();
    }

    populateFeatures() {
        this._featureService.getFeatures()
            .subscribe(
                features => this.features = features,
                err => this.errorMessage = <any>err);
    }
}