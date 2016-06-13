import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { FeatureSet } from './models'
import * as _ from 'lodash';

@Injectable()
export class FeatureService {

    constructor(private _http: Http) { }

    private getApiUrl = '/Home/Get';
    private putApiUrl = '/Home/Put';

    getFeatures(): Observable<any> {
        return this._http.get(this.getApiUrl)
            .map(this.hydrateFeatures)
            .catch(this.handleError);
    }

    updateFeature(features: FeatureSet): any {
        let body = JSON.stringify(features);
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });

        return this._http.put(this.putApiUrl, body, options)
            .map(this.hydrateSingleFeature)
            .catch(this.handleError);
    }

    private hydrateFeatures(res: Response) {
        let body = res.json();
        //filter
        var result = body ?
            _.map(body, (item: any) => {
                 return { name: item.FeatureDescriptor.Name, status: item.FeatureState };
            }) : {};
        return result;
    }

    private hydrateSingleFeature(res: Response) {
        let body = res.json();
        return body || {};
    }

    private handleError(error: any) {
        let errMsg = (error.message)
            ? error.message
            : error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }
}