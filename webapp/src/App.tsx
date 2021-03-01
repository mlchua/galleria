import { Layout, Menu } from 'antd';
import logo from './logo.svg';
import './App.css';
import { Footer } from 'antd/lib/layout/layout';

const { Header, Content, Sider } = Layout;

function App() {
  return (
    <Layout>
      <Sider
        style={{
          overflow: 'auto',
          height: '100vh',
          position: 'fixed',
          left: 0
        }}
      >
        <Menu theme="dark" mode="inline">
          <Menu.Item key="Test-menu-item">
            Test menu item
          </Menu.Item>
        </Menu>
      </Sider>
      <Layout className="site-layout" style={{ marginLeft: 200 }}>
        <Header className="site-layout-background" style={{ padding: 0 }} />
        <Content style={{ margin: '24px 16px 0', overflow: 'initial' }}>
          <div className="site-layout-background" style-={{ padding: 24, textAlign: 'center' }}>
            Test
          </div>
        </Content>
        <Footer></Footer>
      </Layout>
    </Layout>
  );
}

export default App;
