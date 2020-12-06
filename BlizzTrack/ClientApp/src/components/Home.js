import React, { Component } from 'react';
import { BlizzTrack } from "../blizztrack";
import { ListGroup, Button, Breadcrumb, BreadcrumbItem, Card, CardHeader } from 'reactstrap';
import Select from 'react-select';
import Moment from 'react-moment';
import { Link } from 'react-router-dom';

import './Home.css';

export class Home extends Component {
  static displayName = Home.name;
  constructor() {
    super();

    this.state = {
      summary: undefined,
      actions: {},
    }
  }

  update(seqn) {
    BlizzTrack.Call(`ui/home?seqn=${seqn ?? ""}`).then(data => {
     this.setState({ summary: data });
    });
  }

  componentDidMount() {
    BlizzTrack.Call(`NGPD/summary/seqn`).then(data => {
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
  // tag={Link} to={`/v/${item.product}/${item.flags}?seqn=${item.seqn}`}
  handleInputChange(newValue) {
    var a = this.state.actions;
    Object.keys(a).map(x => a[x] = false);
    this.setState({ actions: a });
    this.update(newValue.value);

    this.props.history.push(`/?seqn=${newValue.value}`)
  }

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
      <div className="col-12">
        {!this.state.summary ? "Loading..." :
          <div>
            <div className="mb-1 p-0">
              <Select
                components={{ Option }}
                onChange={this.handleInputChange.bind(this)}
                placeholder="Select sequence to view" options={this.state.seqn} defaultValue={
                  this.state.seqn.length > 0 ? this.state.seqn[0].value : undefined
                } />
            </div>
            {this.state.summary.map((item) =>
              <Card className="mb-1 flex-row">
                <CardHeader className="text-center border-0">
                  {item.name}<br />
                  <img src="//placehold.it/200" alt="" />
                </CardHeader>
                <ListGroup className="list-group-flush px-1 card-block w-100">
                  {item.items.map((item) => {
                    return <div className="row p-1 game-item" key={`${item.product}_${item.flags}`}>
                      <div className="col-9">
                        <div className="d-flex w-100 justify-content-between">
                          <h5 className="mb-1">{item.name}</h5>
                        </div>
                        <small className="text-muted">
                          <Breadcrumb className="breadcrumb-nav">
                            <BreadcrumbItem active>{item.product}</BreadcrumbItem>
                            <BreadcrumbItem active>{item.flags}</BreadcrumbItem>
                            <BreadcrumbItem active>{item.seqn}</BreadcrumbItem>
                          </Breadcrumb>
                        </small>
                      </div>
                      <div className="col text-right">
                        <Button tag={Link} to={`/${item.product}/${item.flags}?seqn=${item.seqn}`} className="mt-2" outline color="dark">
                          View Manifest
                        </Button>
                      </div>
                    </div>
                  })}
                </ListGroup>
              </Card>
            )}
          </div>
        }
      </div>
    );
  }
}
