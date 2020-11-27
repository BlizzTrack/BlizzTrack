import React, { Component } from 'react'
import { BlizzTrack } from "../blizztrack";
import Select, { components } from 'react-select';
import Moment from 'react-moment';

export class GameInfo extends Component {
    constructor() {
        super();

        this.state = {
            game: undefined,
            seqn: [],
            latest: undefined
        }
    }

    update(seqn) {
        BlizzTrack.Call(`NGPD/${this.props.game}/${this.props.file}?seqn=${seqn}`).then(data => {
            this.setState({ game: data, seqn: this.state.seqn, latest: seqn });
        });
    }

    componentDidUpdate() {
        if(this.state.latest == undefined) return;
        if(this.state.latest === this.props.seqn) return;

        this.update(this.props.seqn);
    }

    componentDidMount() {
        BlizzTrack.Call(`NGPD/${this.props.game}/seqn?filter=${this.props.file}`).then(data => {
            var a = data.data.map(a => {
                return {
                    label: a.seqn,
                    value: a.seqn,
                    indexed: a.indexed
                }
            });

            this.setState({
                seqn: a,
            });

            this.update(a[0].value);
        });
    }

    handleInputChange(newValue) {
        this.props.history.push({ search: `?seqn=${newValue.value}` });
        this.update(newValue.value);
    }

    /*
    <li class="list-group-item d-flex justify-content-between align-items-center">
    Cras justo odio
    <span class="badge badge-primary badge-pill">14</span>
  </li>
  */
    render() {
        const Option = props => {
            const { innerProps, innerRef } = props;
            return (
                <div ref={innerRef} {...innerProps} className="px-2 mx-1 hover-me menu-item">
                    <h5 className="m-0" style={{ fontWeight: "normal", fontSize: "1.1rem" }}>{props.data.label}</h5>
                    <small className="text-muted text-small" style={{ fontSize: "0.7rem" }}>
            Indexed <Moment fromNow>{new Date(props.data.indexed + "Z")}</Moment>
                    </small>
                </div>
            );
        };

        return (
            <div>
                {!this.state.game && this.state.seqn > 0 ? "Loading..." :
                    <div className="mt-1">
                        <div className="mb-1">
                            <Select
                                components={{ Option }}
                                onChange={this.handleInputChange.bind(this)} 
                                placeholder="Select sequence to view" options={this.state.seqn} defaultValue={
                                    this.state.seqn.length > 0 ? this.state.seqn[0].value : undefined
                                } />
                        </div>
                        <pre>
                            {JSON.stringify(this.state.game, null, 2)}
                        </pre>
                    </div>
                }
            </div>
        )
    }
}
