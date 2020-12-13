import React, { Component } from 'react'
import { BlizzTrack } from "../blizztrack";
import { TabContent, TabPane, Nav, NavItem, NavLink, Row, Jumbotron } from 'reactstrap';
import classnames from 'classnames';
import queryString from 'query-string';
import { Versions } from '../objects/versions';

import "./ManfestViewer.css";

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

    renderSwitch(file, content) {
        switch (file) {
            case "versions":
            case "bgdl":
                return <Versions content={content} />;
        }
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
                                            {item.flags}
                                        </NavLink>
                                    </NavItem>
                                )}
                            </Nav>
                            <TabContent activeTab={this.state.activeTab}>
                                {Object.entries(this.state.meta.files).map(([key, value]) =>
                                    <TabPane tabId={key}>
                                        <Row className="mt-1">
                                            {
                                                this.renderSwitch(key, value)
                                            }
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
