import { Component, Input } from '@angular/core';
//import { FeatureService } from './featureService';
import { Feature, FeatureSet, FeatureStatus } from './models';

@Component({
    selector: 'featureItem',
    template: ` 
            <div> 
                <label>{{ feature.name }}</label>
                <label>{{ feature.status }}</label>
                <button (click)="toggleStatus(feature)">Toggle Status</button>
            </div>
    `,
})

export class FeatureItem {
    @Input() feature;

    //constructor(private _featureService: FeatureService) { }

    //toggleStatus(f: Feature): void {

    //    let enableFeatures = [];
    //    if (f.status == FeatureStatus.Enabled) enableFeatures.push(f.name);

    //    let disableFeatures = [];
    //    if (f.status == FeatureStatus.Disabled) disableFeatures.push(f.name);

    //    let featureSet = new FeatureSet(enableFeatures, disableFeatures);

    //    this._featureService.updateFeature(featureSet)
    //        .subscribe(feature => f.status = feature.Status);
    //}
}