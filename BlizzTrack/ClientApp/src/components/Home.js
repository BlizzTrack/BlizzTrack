import React, { Component } from 'react';
import { BlizzTrack } from "../blizztrack";
import { ListGroup, Button, Breadcrumb, BreadcrumbItem, Card, CardHeader } from 'reactstrap';
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

  render() {
    return (
      <div className="col-12">
        {!this.state.summary ? "Loading..." :
          <div>
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
                            {item.flags.map((item) =>
                              <BreadcrumbItem active>
                                {item.file} ({item.seqn})
                                  </BreadcrumbItem>
                            )}
                          </Breadcrumb>
                        </small>
                      </div>
                      <div className="col text-right">
                        <Button tag={Link} to={`/${item.product}/manifests`} className="mt-2" outline color="dark">
                          View Manifests
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
