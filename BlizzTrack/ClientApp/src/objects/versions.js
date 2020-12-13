import React, { Component } from 'react'
import Moment from 'react-moment';
import { Col, Row, Breadcrumb, BreadcrumbItem, ListGroup, ListGroupItem, ListGroupItemHeading, ListGroupItemText } from 'reactstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faEquals, faNotEqual } from '@fortawesome/free-solid-svg-icons'

export class Versions extends Component {
    render() {
        const content = this.props.content;
        return (
            <React.Fragment>
                <pre style={{ display: "none" }}>
                    {JSON.stringify(content, undefined, 2)}
                </pre>

                <Col sm="12">
                    {this.buildDiff(content.latest, content.previous)}
                </Col>

            </React.Fragment>
        )
    }

    buildItem(item) {
        return <ListGroup className="mb-1">
            <ListGroupItem className="py-1">
                <ListGroupItemHeading>{item.region}</ListGroupItemHeading>
                <ListGroupItemText className="mb-0">Region</ListGroupItemText>
            </ListGroupItem>
            <ListGroupItem className="py-1">
                <ListGroupItemHeading>{item.versionsname}</ListGroupItemHeading>
                <ListGroupItemText className="mb-0">Version</ListGroupItemText>
            </ListGroupItem>
            <ListGroupItem className="py-1">
                <ListGroupItemHeading>{item.buildconfig}</ListGroupItemHeading>
                <ListGroupItemText className="mb-0">Build Config</ListGroupItemText>
            </ListGroupItem>
            <ListGroupItem className="py-1">
                <ListGroupItemHeading>{item.cdnconfig}</ListGroupItemHeading>
                <ListGroupItemText className="mb-0">CDN Config</ListGroupItemText>
            </ListGroupItem>
            <ListGroupItem className="py-1">
                <ListGroupItemHeading>{item.productconfig}</ListGroupItemHeading>
                <ListGroupItemText className="mb-0">Product Config</ListGroupItemText>
            </ListGroupItem>
        </ListGroup>
    }

    buildDiff(latest, previous) {
        const latestItems = latest.content;
        const previousItems = previous.content;
        return <React.Fragment>
            <Row style={{ borderBottom: "1px solid lightgrey" }}>
                <Col sm="5" className="text-center mx-auto">
                    <Breadcrumb className="breadcrumb-nav">
                        <BreadcrumbItem active>Latest</BreadcrumbItem>
                        <BreadcrumbItem active>Indexed <Moment fromNow>{latest.indexed}</Moment></BreadcrumbItem>
                        <BreadcrumbItem active>{latest.seqn}</BreadcrumbItem>
                    </Breadcrumb>
                </Col>
                <Col sm="2" className={`text-center`}>
                    Changes
                </Col>
                <Col sm="5">
                    <Breadcrumb className="breadcrumb-nav">
                        <BreadcrumbItem active>Previous</BreadcrumbItem>
                        <BreadcrumbItem active>Indexed <Moment fromNow>{previous.indexed}</Moment></BreadcrumbItem>
                        <BreadcrumbItem active>{previous.seqn}</BreadcrumbItem>
                    </Breadcrumb>
                </Col>
            </Row>
            {latestItems.map((item, index) => <Row className="mb-2 " style={{ borderBottom: "1px solid lightgrey" }}>
                {
                    Object.entries(latestItems[index]).map(([key, value]) =>
                        <React.Fragment>
                            <Col sm="5">
                                <div className="py-1 border-0 list-group-item" style={{ background: "transparent" }}>
                                    <ListGroupItemHeading>{value}</ListGroupItemHeading>
                                    <ListGroupItemText className="mb-0">{key}</ListGroupItemText>
                                </div>
                            </Col>
                            {previousItems.length > 0 ?
                                <React.Fragment>
                                    <Col sm="2" className={`text-center display-4 ${value === previousItems[index][key] ? "" : "text-danger"}`}>
                                        {value === previousItems[index][key] ?
                                            <FontAwesomeIcon icon={faEquals} />
                                            :
                                            <FontAwesomeIcon icon={faNotEqual} />
                                        }
                                    </Col>
                                    <Col sm="5">
                                        <div className="py-1 border-0 list-group-item" style={{ background: "transparent" }}>
                                            <ListGroupItemHeading>{previousItems[index][key]}</ListGroupItemHeading>
                                            <ListGroupItemText className="mb-0">{key}</ListGroupItemText>
                                        </div>
                                    </Col>
                                </React.Fragment> : <React.Fragment>
                                    <Col sm="2" className={`text-center display-4`}>
                                       N/A
                                    </Col>
                                    <Col sm="5">
                                        <div className="py-1 border-0 list-group-item" style={{ background: "transparent" }}>
                                            <ListGroupItemHeading>N/A</ListGroupItemHeading>
                                            <ListGroupItemText className="mb-0">{key}</ListGroupItemText>
                                        </div>
                                    </Col>
                                </React.Fragment>}
                        </React.Fragment>
                    )
                }
            </Row>)
            }
        </React.Fragment>
    }
}
