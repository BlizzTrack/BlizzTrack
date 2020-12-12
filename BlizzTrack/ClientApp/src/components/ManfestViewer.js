import React, { Component } from 'react'
import { BlizzTrack } from "../blizztrack";
import { TabContent, TabPane, Nav, NavItem, NavLink, Table, Row, Col, Jumbotron, Breadcrumb, BreadcrumbItem } from 'reactstrap';
import classnames from 'classnames';
import Moment from 'react-moment';
import queryString from 'query-string';

export class ManfestViewer extends Component {
    constructor() {
        super();

        this.state = {
            meta: undefined,
            activeTab: undefined,
        }
    }

    switchTab(tab) {
        if (this.state.activeTab !== tab) {
            this.setState({
                activeTab: tab,
                meta: this.state.meta
            });
        }
    }

    update() {
        this.url = queryString.parse(this.props.location.search);

        BlizzTrack.Call(`ui/game/${this.props.match.params.game}`).then(data => {
            this.setState({
                meta: data,
                activeTab: this.url.file ?? data.metadata.files[0].flags
            });
        });
    }

    componentDidMount() {
        this.update();
    }

    render() {
        return (
            <div>
                <a href="/" className="mb-2">Back to all games</a>
                {!this.state.meta ? <div className="text-center mt-4"></div> :
                    <div>
                        <Jumbotron className="my-2 text-center">
                            <h1 className="display-3">
                                {this.state.meta.metadata.name}
                            </h1>
                        </Jumbotron>
                        <div>
                            <Nav tabs className="nav-justified">
                                {this.state.meta.metadata.files.map(item =>
                                    <NavItem>
                                        <NavLink
                                            className={classnames({ active: this.state.activeTab === item.flags })}
                                            onClick={() => { this.switchTab(item.flags); }}
                                        >
                                            {item.flags} ({item.seqn})
                                        </NavLink>
                                    </NavItem>
                                )}
                            </Nav>
                            <TabContent activeTab={this.state.activeTab}>
                                {Object.entries(this.state.meta.files).map(([key, value]) =>
                                    <TabPane tabId={key}>
                                        <Row>
                                            <Col style={{ overflow: value.content.length > 0 ? "auto" : "hidden" }}>
                                                <Breadcrumb className="breadcrumb-nav mt-2 mb-2 text-muted">
                                                    <BreadcrumbItem active>{value.seqn}</BreadcrumbItem>
                                                    <BreadcrumbItem active>{key}</BreadcrumbItem>
                                                    <BreadcrumbItem active>Indexed <Moment fromNow>{value.indexed}</Moment></BreadcrumbItem>
                                                    <BreadcrumbItem active><Moment>{value.indexed}</Moment></BreadcrumbItem>
                                                </Breadcrumb>
                                                {value.content.length > 0 ?
                                                    <Table striped responsive className="m-0">
                                                        <thead>
                                                            <tr className="text-center">
                                                                {Object.keys(value.content[0]).map(item => <th>{item}</th>)}
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            {Object.entries(value.content).map(([key, value]) =>
                                                                <tr>
                                                                    {Object.entries(value).map(([key, value]) =>
                                                                        <td>{value instanceof Array ? <ul>{value.map(item => <li>{item}</li>)}</ul> : value === "versions" ? "N/A" : value}</td>
                                                                    )}
                                                                </tr>
                                                            )}
                                                        </tbody>
                                                    </Table>
                                                    : <div className="display-4 text-center h-25">Manifest empty</div>
                                                }
                                            </Col>
                                        </Row>
                                    </TabPane>
                                )}
                            </TabContent>
                        </div>
                    </div>
                }
            </div>
        )
    }
}
