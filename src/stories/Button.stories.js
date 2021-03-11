import React from 'react';

import { Button } from './Button';

import * as ModalWindow from './../MyPlanner.Client.View/fable-output/Components/ModalWindow'

ModalWindow.ensureDefined();

export default {
    title: 'Example/Button',
    component: Button,
    argTypes: {
        backgroundColor: { control: 'color' },
    },
};

const Template = (args) => <Button {...args} />;

const Div = (args) =>
    <modal-window visible onclose='alert(this.tagName)'>
        <span slot="title"><button onclick="this.parentNode.parentNode.close()">Title Button</button></span>
        <span slot="description">{args.label}</span>
    </modal-window>

export const Primary = Template.bind({});
Primary.args = {
    primary: true,
    label: 'Button',
};

export const Secondary = Template.bind({});
Secondary.args = {
    label: 'Button',
};

export const Large = Template.bind({});
Large.args = {
    size: 'large',
    label: 'Button',
};

export const Small = Template.bind({});
Small.args = {
    size: 'small',
    label: 'Button',
};

export const Modal = Div.bind({});
Modal.args = {
    primary: true,
    label: 'Onur',
};